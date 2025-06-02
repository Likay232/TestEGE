using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AdminService(DataComponent component)
{
    /*
    public async Task<List<UserDto>> GetUsers()
    {
        return await component.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
            })
            .ToListAsync();
    }

    public async Task<bool> SwitchBlockState(int userId)
    {
        var userToBlock = await component.Users.FirstOrDefaultAsync(u => u.Id == userId);
        
        if (userToBlock == null)
            throw new Exception("Пользователь не найден.");
        
        userToBlock.isBlocked = !userToBlock.isBlocked;
        
        return await component.Update(userToBlock);
    }

    public async Task<bool> ChangeUserPassword(ChangePassword request)
    {
        var userEntry = await component.Users.FirstOrDefaultAsync(u => u.Id == request.userId);

        if (userEntry == null)
            throw new Exception("Пользователь с заданным Id не найден");

        userEntry.Password = request.newPassword;

        return await component.Update(userEntry);
    }

    public async Task<List<ThemeDto>> GetThemes()
    {
        return await component.Themes.Select(t => new ThemeDto
        {
            Id = t.Id,
            Description = t.Description,
            Title = t.Title
        }).ToListAsync();
    }

    public async Task<bool> CreateNewTheme(CreateTheme request)
    {
        var newTheme = new Theme
        {
            Title = request.Title,
            Description = request.Description,
        };

        return await component.Insert(newTheme);
    }

    public async Task<bool> AddTaskForTheme(TaskDto taskToAdd)
    {
        if (!component.Themes.Any(t => t.Id == taskToAdd.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");

        var newTask = new TaskForTest()
        {
            ThemeId = taskToAdd.ThemeId,
            Text = taskToAdd.Text,
            CorrectAnswer = taskToAdd.CorrectAnswer,
            DifficultyLevel = taskToAdd.DifficultyLevel,
            ImageData = taskToAdd.ImageData,
            FileData = taskToAdd.FileData,
        };

        return await component.Insert(newTask);
    }

    public async Task<bool> EditTaskForTheme(TaskDto updatedTask)
    {
        if (!component.Themes.Any(t => t.Id == updatedTask.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");

        var taskToEdit = component.Tasks.FirstOrDefault(t => t.Id == updatedTask.Id);

        if (taskToEdit == null)
            throw new Exception("Задание с таким Id не найдено.");

        taskToEdit.Text = updatedTask.Text;
        taskToEdit.CorrectAnswer = updatedTask.CorrectAnswer;
        taskToEdit.DifficultyLevel = updatedTask.DifficultyLevel;
        taskToEdit.ImageData = updatedTask.ImageData;
        taskToEdit.FileData = updatedTask.FileData;
        taskToEdit.ThemeId = updatedTask.ThemeId;

        return await component.Update(taskToEdit);
    }

    public async Task<bool> DeleteTaskForTheme(int taskId)
    {
        return await component.Delete<TaskForTest>(taskId);
    }

    public async Task<bool> AddLessonForTheme(LessonDto lessonToAdd)
    {
        if (!component.Themes.Any(t => t.Id == lessonToAdd.ThemeId))
            throw new Exception("Тема с таким Id не найдена.");

        var newLesson = new Lesson
        {
            ThemeId = lessonToAdd.ThemeId,
            Text = lessonToAdd.Text,
            Link = lessonToAdd.Link
        };

        return await component.Insert(newLesson);
    }

    public async Task<List<TaskDto>> GetTasks()
    {
        return await component.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Text = t.Text,
                CorrectAnswer = t.CorrectAnswer,
                DifficultyLevel = t.DifficultyLevel,
                ImageData = t.ImageData,
                FileData = t.FileData,
            })
            .ToListAsync();
    }

    public async Task<string> CreateTest(CreateTest request)
    {
        var newTest = new Variant
        {
            Title = request.Title,
        };

        if (!await component.Insert(newTest))
            throw new Exception("Ошибка создания теста.");

        List<int> failedTaskIds = new List<int>();

        foreach (var taskId in request.taskIds)
        {
            var result = await component.Insert(new TestTask
            {
                TestId = newTest.Id,
                TaskForTestId = taskId
            });

            if (!result)
            {
                failedTaskIds.Add(taskId);
            }
        }

        if (failedTaskIds.Count > 0)
        {
            return $"Не удалось добавить задачи с ID: {string.Join(", ", failedTaskIds)}";
        }

        return $"Тест успешно создан. ID: {newTest.Id}";
    }
    */
}