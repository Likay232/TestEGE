﻿using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class StudentService(DataComponent component, FileService fileService)
{
    public async Task<ProfileInfo> GetProfileInfo(int userId)
    {
        return await component.Users
            .Select(u => new ProfileInfo
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Password = u.Password,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                About = u.About
            })
            .FirstAsync(x => x.Id == userId);
    }

    public async Task<bool> EditProfileInfo(ProfileInfo profileInfo)
    {
        var userEntry = await component.Users
            .FirstOrDefaultAsync(u => u.Id == profileInfo.Id);
        if (userEntry == null) return false;

        userEntry.FirstName = profileInfo.FirstName;
        userEntry.LastName = profileInfo.LastName;
        userEntry.Email = profileInfo.Email;
        userEntry.Password = profileInfo.Password;
        userEntry.DateOfBirth = DateTime.SpecifyKind(profileInfo.DateOfBirth, DateTimeKind.Utc);
        userEntry.TimeZone = profileInfo.TimeZone;
        userEntry.About = profileInfo.About;

        return await component.Update(userEntry);
    }

    public async Task<List<ExerciseDto>> GetAllExercises()
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.ModerationPassed)
            .Select(ex => new ExerciseDto
            {
                Id = ex.Id,
                Text = ex.Text,
                Answer = ex.Answer,
                Year = ex.Year,
                ExerciseFilePath = ex.ExerciseFilePath,
                SolutionFilePath = ex.SolutionFilePath,
                EgeNumber = ex.EgeNumber,
                AttachmentRequired = ex.AttachmentRequired,
                ModerationPassed = ex.ModerationPassed,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<ExerciseDto> GetExercise(int exerciseId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.Id == exerciseId)
            .Select(ex => new ExerciseDto
            {
                Id = ex.Id,
                Text = ex.Text,
                Answer = ex.Answer,
                PrimaryScore = ex.PrimaryScore,
                Year = ex.Year,
                ExerciseFilePath = ex.ExerciseFilePath,
                SolutionFilePath = ex.SolutionFilePath,
                EgeNumber = ex.EgeNumber,
                AttachmentRequired = ex.AttachmentRequired,
                ModerationPassed = ex.ModerationPassed,
                TeacherId = ex.TeacherId,
                TeacherFirstName = ex.Teacher.FirstName,
                TeacherLastName = ex.Teacher.LastName,
            })
            .FirstAsync();
    }


    public async Task<List<VariantDto>> GetAllVariants()
    {
        return await component.Variants
            .Include(v => v.Teacher)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<VariantDto> GetVariant(int variantId)
    {
        if (!await component.Variants.AnyAsync(v => v.Id == variantId))
            throw new Exception("Вариант не найден.");

        var exercises = await GetExercisesForVariant(variantId);
        var assignedStudents = await GetAssignedStudentsForVariant(variantId);

        return await component.Variants
            .Where(v => v.Id == variantId)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherId = v.TeacherId,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
                Exercises = exercises,
                AssignedUsers = assignedStudents
            })
            .FirstAsync();
    }

    private async Task<List<ExerciseDto>> GetExercisesForVariant(int variantId)
    {
        return await component.VariantExercises
            .Include(v => v.Exercise)
            .ThenInclude(e => e.Teacher)
            .Where(v => v.VariantId == variantId)
            .Select(v => new ExerciseDto
            {
                Id = v.ExerciseId,
                EgeNumber = v.Exercise.EgeNumber,
                Text = v.Exercise.Text,
                Year = v.Exercise.Year,
                AttachmentRequired = v.Exercise.AttachmentRequired,
                ExerciseFilePath = v.Exercise.ExerciseFilePath,
                TeacherFirstName = v.Exercise.Teacher.FirstName,
                TeacherLastName = v.Exercise.Teacher.LastName,
            })
            .ToListAsync();
    }

    private async Task<List<UserDto>> GetAssignedStudentsForVariant(int variantId)
    {
        return await component.VariantAssignments
            .Include(v => v.Student)
            .Where(v => v.VariantId == variantId)
            .Select(va => new UserDto
            {
                Id = va.StudentId,
                Email = va.Student.Email,
                FirstName = va.Student.FirstName,
                LastName = va.Student.LastName,
            })
            .ToListAsync();
    }

    public async Task<List<VariantDto>> GetAssignedVariants(int studentId)
    {
        return await component.VariantAssignments
            .Include(v => v.Variant)
            .ThenInclude(v => v.Teacher)
            .Where(v => v.StudentId == studentId)
            .Select(v => new VariantDto
            {
                Id = v.VariantId,
                Title = v.Variant.Title,
                TeacherFirstName = v.Variant.Teacher.FirstName,
                TeacherLastName = v.Variant.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<CheckedVariant> CheckVariant(VariantForCheck variant)
    {
        var wrongTasks = new List<WrongExercise>();
        int primaryScore = 0;

        foreach (var userAnswer in variant.Answers)
        {
            var isCorrect = await CheckExercise(new CheckExercise
            {
                ExerciseId = userAnswer.ExerciseId,
                UserId = variant.StudentId,
                Answer = userAnswer.Answer ?? "",
                Attachment = userAnswer.Attachment,
            });

            var task = await component.Exercises
                .Include(e => e.Teacher)
                .FirstAsync(e => e.Id == userAnswer.ExerciseId);

            if (!isCorrect)
            {
                wrongTasks.Add(new WrongExercise
                {
                    Text = task.Text,
                    Year = task.Year,
                    Answer = userAnswer.Answer ?? "",
                    ExerciseFilePath = task.ExerciseFilePath,
                    SolutionFilePath = task.SolutionFilePath,
                    EgeNumber = task.EgeNumber,
                    TeacherId = task.TeacherId,
                    TeacherLastName = task.Teacher.LastName,
                    TeacherFirstName = task.Teacher.FirstName,
                });
            }
            else
            {
                primaryScore += task.PrimaryScore;
            }
        }

        return new CheckedVariant
        {
            WrongExercises = wrongTasks,
            Score = $"{primaryScore}",
            SecondaryScore = $"{ConvertPrimaryScoreToSecondaryScore(primaryScore)}"
        };
    }

    private int ConvertPrimaryScoreToSecondaryScore(int primaryScore) =>
        primaryScore switch
        {
            0 => 0,
            1 => 7,
            2 => 14,
            3 => 20,
            4 => 27,
            5 => 34,
            6 => 40,
            7 => 43,
            8 => 46,
            9 => 48,
            10 => 51,
            11 => 54,
            12 => 56,
            13 => 59,
            14 => 62,
            15 => 64,
            16 => 67,
            17 => 70,
            18 => 72,
            19 => 75,
            20 => 78,
            21 => 80,
            22 => 83,
            23 => 85,
            24 => 88,
            25 => 90,
            26 => 93,
            27 => 95,
            28 => 98,
            >= 29 => 100,
            _ => 0
        };

    private async Task<bool> CheckExercise(CheckExercise answer)
    {
        var task = await component.Exercises.FirstOrDefaultAsync(t => t.Id == answer.ExerciseId);

        if (task == null) return false;

        var existing = await component.StudentExercises
            .FirstOrDefaultAsync(ce => ce.StudentId == answer.UserId && ce.ExerciseId == answer.ExerciseId);

        var isCorrect = task.Answer == answer.Answer;

        if (existing != null)
        {
            var answerChanged = existing.StudentAnswer != answer.Answer;

            existing.StudentAnswer = answer.Answer;
            existing.IsCorrect = isCorrect;

            if (answer.Attachment != null)
            {
                var newExerciseFileName =
                    $"{existing.Id}_studentSolution{Path.GetExtension(answer.Attachment.FileName)}";
                await fileService.SaveFileToRepo(answer.Attachment, newExerciseFileName);
                existing.StudentSolutionFilePath = newExerciseFileName;
            }
            else if (answerChanged)
                existing.StudentSolutionFilePath = "";

            await component.Update(existing);
        }
        else
        {
            var studentExerciseToAdd = new StudentExercise
            {
                ExerciseId = answer.ExerciseId,
                StudentId = answer.UserId,
                StudentAnswer = answer.Answer,
                StudentSolutionFilePath = "",
                IsCorrect = isCorrect,
            };

            await component.Insert(studentExerciseToAdd);

            if (answer.Attachment != null)
            {
                var newExerciseFileName =
                    $"{studentExerciseToAdd.Id}_studentSolution{Path.GetExtension(answer.Attachment.FileName)}";
                await fileService.SaveFileToRepo(answer.Attachment, newExerciseFileName);
                studentExerciseToAdd.StudentSolutionFilePath = newExerciseFileName;

                await component.Update(studentExerciseToAdd);
            }
        }

        return isCorrect;
    }

    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        return await fileService.GetFileBytes(fileName);
    }

    public async Task<bool> DeleteAccount(int studentId)
    {
        await DeleteByForeignKey(component.StudentExercises, studentId, "StudentId");
        await DeleteByForeignKey(component.GroupStudents, studentId, "StudentId");
        await DeleteByForeignKey(component.ResetPasses, studentId, "UserId");
        await DeleteByForeignKey(component.VariantAssignments, studentId, "StudentId");
        await DeleteByForeignKey(component.BlockUsers, studentId, "UserId");

        return await component.Delete<User>(studentId);
    }

    private async Task DeleteByForeignKey<T>(
        IQueryable<T> queryable,
        int foreignKeyValue,
        string foreignKeyName,
        Func<int, Task>? deleteFunc = null
    ) where T : class
    {
        var ids = await queryable
            .Where(e => EF.Property<int>(e, foreignKeyName) == foreignKeyValue)
            .Select(e => EF.Property<int>(e, "Id"))
            .ToListAsync();

        foreach (var id in ids)
        {
            if (deleteFunc != null)
                await deleteFunc(id);
            else
                await component.Delete<T>(id);
        }
    }
}