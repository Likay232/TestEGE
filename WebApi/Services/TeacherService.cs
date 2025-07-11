﻿using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class TeacherService(DataComponent component, FileService fileService)
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

    public async Task<List<ExerciseDto>> GetMyExercises(int userId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.TeacherId == userId && ex.ModerationPassed)
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

    public async Task<EditExercise> GetExerciseToEdit(int exerciseId)
    {
        return await component.Exercises
            .Include(ex => ex.Teacher)
            .Where(ex => ex.Id == exerciseId)
            .Select(ex => new EditExercise
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

    public async Task<bool> EditExercise(EditExercise updatedExercise)
    {
        var exerciseEntry = await component.Exercises.FirstOrDefaultAsync(e => e.Id == updatedExercise.Id);

        if (exerciseEntry == null)
            throw new Exception("Задание с таким Id не найдено.");

        exerciseEntry.Text = updatedExercise.Text;
        exerciseEntry.Answer = updatedExercise.Answer;
        exerciseEntry.PrimaryScore = updatedExercise.PrimaryScore;
        exerciseEntry.Year = updatedExercise.Year;
        exerciseEntry.EgeNumber = updatedExercise.EgeNumber;
        exerciseEntry.AttachmentRequired = updatedExercise.AttachmentRequired;
        exerciseEntry.TeacherId = updatedExercise.TeacherId;
        exerciseEntry.ModerationPassed = updatedExercise.ModerationPassed;

        if (updatedExercise.ExerciseFile != null)
        {
            var newExerciseFileName =
                $"{exerciseEntry.Id}_exercise{Path.GetExtension(updatedExercise.ExerciseFile.FileName)}";
            await fileService.SaveFileToRepo(updatedExercise.ExerciseFile, newExerciseFileName);
            exerciseEntry.ExerciseFilePath = newExerciseFileName;
        }

        if (updatedExercise.SolutionFile != null)
        {
            var newSolutionFileName =
                $"{exerciseEntry.Id}_solution{Path.GetExtension(updatedExercise.SolutionFile.FileName)}";
            await fileService.SaveFileToRepo(updatedExercise.SolutionFile, newSolutionFileName);
            exerciseEntry.SolutionFilePath = newSolutionFileName;
        }

        return await component.Update(exerciseEntry);
    }

    public async Task<bool> AddExercise(AddExercise request)
    {
        var newEntry = new Exercise
        {
            Text = request.Text,
            Answer = request.Answer,
            PrimaryScore = request.PrimaryScore,
            Year = request.Year,
            ExerciseFilePath = "",
            SolutionFilePath = "",
            EgeNumber = request.EgeNumber,
            AttachmentRequired = request.AttachmentRequired,
            TeacherId = request.TeacherId,
            ModerationPassed = false,
        };

        var inserted = await component.Insert(newEntry);
        if (!inserted)
            return false;

        var updated = false;

        if (request.ExerciseFile != null)
        {
            var exerciseFileName = $"{newEntry.Id}_exercise{Path.GetExtension(request.ExerciseFile.FileName)}";
            await fileService.SaveFileToRepo(request.ExerciseFile, exerciseFileName);
            newEntry.ExerciseFilePath = exerciseFileName;
            updated = true;
        }

        if (request.SolutionFile != null)
        {
            var solutionFileName = $"{newEntry.Id}_solution{Path.GetExtension(request.SolutionFile.FileName)}";
            await fileService.SaveFileToRepo(request.SolutionFile, solutionFileName);
            newEntry.SolutionFilePath = solutionFileName;
            updated = true;
        }

        if (updated)
            await component.Update(newEntry);

        return true;
    }

    public async Task<List<VariantDto>> GetMyVariants(int teacherId)
    {
        return await component.Variants
            .Include(v => v.Teacher)
            .Where(v => v.TeacherId == teacherId)
            .Select(v => new VariantDto
            {
                Id = v.Id,
                Title = v.Title,
                TeacherFirstName = v.Teacher.FirstName,
                TeacherLastName = v.Teacher.LastName,
            })
            .ToListAsync();
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

    public async Task<bool> AddVariant(AddVariant request)
    {
        var newVariant = new Variant
        {
            Title = request.Title,
            TeacherId = request.TeacherId,
        };

        await component.Insert(newVariant);

        var exerciseIds = request.Exercises;

        if (exerciseIds == null || exerciseIds.Count == 0)
        {
            var maxTasks = await component.Exercises
                .Where(e => e.TeacherId == request.TeacherId)
                .CountAsync();

            var random = new Random();
            var amount = random.Next(1, maxTasks + 1);

            exerciseIds = await GetRandomExercisesForVariant(amount, request.TeacherId);
        }

        foreach (var exercise in exerciseIds)
        {
            var newVariantExercise = new VariantExercise
            {
                VariantId = newVariant.Id,
                ExerciseId = exercise,
            };

            await component.Insert(newVariantExercise);
        }

        if (request.AssignedUsers != null)
            foreach (var user in request.AssignedUsers)
            {
                var newAssignment = new VariantAssignment
                {
                    VariantId = newVariant.Id,
                    StudentId = user,
                    TeacherId = newVariant.TeacherId,
                };

                await component.Insert(newAssignment);
            }

        return true;
    }

    private async Task<List<int>> GetRandomExercisesForVariant(int amount, int teacherId)
    {
        var random = new Random();

        var allExercises = await component.Exercises.ToListAsync();

        return allExercises
            .Where(e => e.TeacherId ==  teacherId)
            .OrderBy(x => random.Next())
            .Take(amount)
            .Select(x => x.Id)
            .ToList();
    }

    public async Task<bool> EditVariant(EditVariant updatedVariant)
    {
        var variantEntry = await component.Variants
            .Where(v => v.Id == updatedVariant.Id)
            .FirstOrDefaultAsync();

        if (variantEntry == null) throw new Exception("Вариант с заданным Id не найден.");

        var currentVariantExercises = await GetExercisesForVariant(updatedVariant.Id);
        var currentAssignedStudents = await GetAssignedStudentsForVariant(updatedVariant.Id);

        foreach (var exercise in currentVariantExercises)
        {
            if (!updatedVariant.ExerciseIds.Exists(id => id == exercise.Id))
            {
                await DeleteByForeignKey(component.VariantExercises, exercise.Id, "ExerciseId");
            }
        }

        foreach (var exerciseId in updatedVariant.ExerciseIds)
        {
            if (currentVariantExercises.All(e => e.Id != exerciseId))
            {
                await component.Insert(new VariantExercise
                {
                    VariantId = variantEntry.Id,
                    ExerciseId = exerciseId
                });
            }
        }

        foreach (var assignment in currentAssignedStudents)
        {
            if (updatedVariant.StudentIds.All(id => id != assignment.Id))
            {
                var variantAssignmentEntry = await component.VariantAssignments
                    .Include(v => v.Variant)
                    .Include(v => v.Student)
                    .Where(v => v.VariantId == variantEntry.Id && v.StudentId == assignment.Id)
                    .FirstOrDefaultAsync();

                if (variantAssignmentEntry == null) continue;

                await component.Delete<VariantAssignment>(variantAssignmentEntry.Id);
            }
        }

        foreach (var studentId in updatedVariant.StudentIds)
        {
            if (currentAssignedStudents.All(a => a.Id != studentId))
            {
                await component.Insert(new VariantAssignment
                {
                    VariantId = updatedVariant.Id,
                    TeacherId = updatedVariant.TeacherId,
                    StudentId = studentId
                });
            }
        }

        variantEntry.Title = updatedVariant.Title;

        return await component.Update(variantEntry);
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
                Year = v.Exercise.Year,
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

    public async Task<List<UserDto>> GetStudents()
    {
        return await component.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == "Student")
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .ToListAsync();
    }

    public async Task<UserDto> GetStudent(int userId)
    {
        if (!await component.Users.AnyAsync(u => u.Id == userId))
            throw new Exception("Пользователь с заданным Id не найден.");

        return await component.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                About = u.About
            })
            .FirstAsync(u => u.Id == userId);
    }

    public async Task<List<GroupDto>> GetGroups(int teacherId)
    {
        return await component.Groups
            .Include(g => g.Teacher)
            .Where(g => g.TeacherId == teacherId)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                TeacherFirstName = g.Teacher.FirstName,
                TeacherLastName = g.Teacher.LastName,
            })
            .ToListAsync();
    }

    public async Task<GroupDto> GetGroup(int groupId)
    {
        var group = await component.Groups
            .Include(g => g.Teacher)
            .Select(g => new GroupDto
            {
                Id = g.Id,
                Name = g.Name,
                TeacherFirstName = g.Teacher.FirstName,
                TeacherLastName = g.Teacher.LastName,
            })
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) throw new Exception("Группа с данным Id не найдена.");

        var students = await GetAssignedStudentsForGroup(groupId);

        group.Students = students;

        return group;
    }

    private async Task<List<UserDto>> GetAssignedStudentsForGroup(int groupId)
    {
        return await component.GroupStudents
            .Include(g => g.Student)
            .Where(g => g.GroupId == groupId)
            .Select(g => new UserDto
            {
                Id = g.StudentId,
                FirstName = g.Student.FirstName,
                LastName = g.Student.LastName,
                Email = g.Student.Email
            })
            .ToListAsync();
    }

    public async Task<bool> EditGroup(EditGroup group)
    {
        if (!await component.Groups.AnyAsync(g => g.Id == group.GroupId))
            return false;

        var groupEntry = await component.Groups
            .FirstAsync(g => g.Id == group.GroupId);

        groupEntry.Name = group.GroupName;
        await component.Update(groupEntry);

        var currentStudents = await component.GroupStudents
            .Where(gs => gs.GroupId == group.GroupId)
            .ToListAsync();

        var newStudentIds = group.StudentIds ?? new List<int>();

        var toRemove = currentStudents
            .Where(link => !newStudentIds.Contains(link.StudentId))
            .ToList();

        foreach (var link in toRemove)
        {
            await component.Delete<GroupStudent>(link.Id);
        }

        var existingIds = currentStudents.Select(x => x.StudentId).ToHashSet();
        var toAdd = newStudentIds
            .Where(id => !existingIds.Contains(id))
            .ToList();

        foreach (var id in toAdd)
        {
            var newEntry = new GroupStudent
            {
                GroupId = group.GroupId,
                StudentId = id
            };

            await component.Insert(newEntry);
        }

        return true;
    }

    public async Task<bool> AddGroup(AddGroup request)
    {
        if (request.StudentIds == null)
            return false;

        var group = new Group
        {
            Name = request.Name,
            TeacherId = request.TeacherId
        };

        await component.Insert(group);

        foreach (var id in request.StudentIds)
        {
            await component.Insert(new GroupStudent
            {
                GroupId = group.Id,
                StudentId = id
            });
        }

        return true;
    }

    public async Task<bool> DeleteVariant(int variantId)
    {
        await DeleteByForeignKey(component.VariantExercises, variantId, "VariantId");
        await DeleteByForeignKey(component.VariantAssignments, variantId, "VariantId");
        return await component.Delete<Variant>(variantId);
    }

    public async Task<bool> DeleteExercise(int exerciseId)
    {
        await DeleteByForeignKey(component.VariantExercises, exerciseId, "ExerciseId");
        await DeleteByForeignKey(component.StudentExercises, exerciseId, "ExerciseId");
        return await component.Delete<Exercise>(exerciseId);
    }

    public async Task<bool> DeleteGroup(int groupId)
    {
        await DeleteByForeignKey(component.GroupStudents, groupId, "GroupId");
        return await component.Delete<Group>(groupId);
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

    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        return await fileService.GetFileBytes(fileName);
    }

    public async Task<List<ExerciseSolutions>> GetStudentSolutions()
    {
        var exercises = await component.Exercises
            .Select(e => new ExerciseSolutions
            {
                Id = e.Id,
                Year = e.Year,
                Text = e.Text,
                Answer = e.Answer,
                EgeNumber = e.EgeNumber,
                StudentSolutions = new List<StudentSolution>()
            }).ToListAsync();

        var studentExercises = await component.StudentExercises
            .Include(se => se.Student)
            .Where(se => exercises.Select(e => e.Id).Contains(se.ExerciseId))
            .Select(se => new
            {
                se.Id,
                se.ExerciseId,
                se.Student.FirstName,
                se.Student.LastName,
                se.StudentAnswer,
                se.StudentSolutionFilePath
            }).ToListAsync();

        foreach (var exercise in exercises)
        {
            exercise.StudentSolutions = studentExercises
                .Where(se => se.ExerciseId == exercise.Id)
                .Select(se => new StudentSolution
                {
                    Id = se.Id,
                    StudentFirstName = se.FirstName,
                    StudentLastName = se.LastName,
                    Answer = se.StudentAnswer,
                    StudentSolutionPath = se.StudentSolutionFilePath
                }).ToList();
        }

        return exercises;
    }

    public async Task<bool> DeleteAccount(int teacherId)
    {
        await DeleteByForeignKey(component.VariantAssignments, teacherId, "TeacherId");
        await DeleteByForeignKey(component.ResetPasses, teacherId, "UserId");
        await DeleteByForeignKey(component.BlockUsers, teacherId, "UserId");

        await DeleteByForeignKey(component.Groups, teacherId, "TeacherId", DeleteGroup);
        await DeleteByForeignKey(component.Exercises, teacherId, "TeacherId", DeleteExercise);
        await DeleteByForeignKey(component.Variants, teacherId, "TeacherId", DeleteVariant);

        return await component.Delete<User>(teacherId);
    }
}