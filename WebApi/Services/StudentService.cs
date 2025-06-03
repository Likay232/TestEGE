using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class StudentService(DataComponent component)
{
    /*
    public async Task<List<TaskForClientDto>> GetTasksForTheme(GetTasks request)
    {
        if (!await component.Themes.AnyAsync(t => t.Id == request.ThemeId))
            throw new Exception("Тема не найдена.");
        
        var completed = await component.CompletedTasks
            .Where(ct => ct.UserId == request.UserId)
            .ToListAsync();

        foreach (var completedTask in completed)
        {
            Console.WriteLine($"Completed Task: {completedTask.Id} - taskId: {completedTask.TaskForTestId} UserID - {completedTask.UserId} Correct - {completedTask.IsCorrect}");
        }

        var tasks = await component.Tasks
            .Where(t => t.ThemeId == request.ThemeId)
            .ToListAsync();

        return tasks.Select(t =>
        {
            var completedTask = completed.FirstOrDefault(ct => ct.TaskForTestId == t.Id);

            return new TaskForClientDto
            {
                Id = t.Id,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                File = t.FileData,
                Image = t.ImageData,
                IsCorrect = completedTask?.IsCorrect ?? false
            };
        }).ToList();
    }

    public async Task<List<LessonDto>> GetLessonsForTheme(int themeId)
    {
        return await component.Lessons
            .Where(l => l.ThemeId == themeId)
            .Select(l => new LessonDto
            {
                Text = l.Text,
                Link = l.Link,
            })
            .ToListAsync();
    }

    public async Task<List<TestDto>> GetTests()
    {
        return await component.Tests
            .Select(t => new TestDto
            {
                Id = t.Id,
                Title = t.Title,
            })
            .ToListAsync();
    }

    public async Task<List<TaskForClientDto>> GetTest(int testId)
    {
        if (!await component.TestTasks.AnyAsync(t => t.Id == testId))
            throw new Exception("Вопросы для теста с заданным Id не найден.");

        return await component.TestTasks
            .Where(t => t.TestId == testId)
            .Select(t => new TaskForClientDto()
            {
                Id = t.Id,
                Text = t.TaskForTest.Text,
                CorrectAnswer = "",
                DifficultyLevel = t.TaskForTest.DifficultyLevel,
                File = t.TaskForTest.FileData,
                Image = t.TaskForTest.ImageData,
                IsCorrect = false,
            })
            .ToListAsync();
    }

    public async Task<CheckedTest> CheckTest(TestForCheck test)
    {
        List<UserAnswer> wrongAnswers = new List<UserAnswer>();
        
        foreach (var userAnswer in test.Answers)
        {
            var task = await component.Tasks.FirstOrDefaultAsync(t => t.Id == userAnswer.TaskId);
            var completedTask = new CompletedTask();

            if (task == null || task.CorrectAnswer != userAnswer.Answer)
            {
                wrongAnswers.Add(userAnswer);
            }
            
            if (task != null)
            {
                completedTask.TaskForTestId = userAnswer.TaskId;
                completedTask.UserId = test.UserId;
                completedTask.IsCorrect = task.CorrectAnswer == userAnswer.Answer;
            }

            await component.Insert(completedTask);
        }

        return new CheckedTest
        {
            WrongAnswers = wrongAnswers,
            Score = $"{test.Answers.Count - wrongAnswers.Count} / {test.Answers.Count}"
        }; 
    }
    */
}