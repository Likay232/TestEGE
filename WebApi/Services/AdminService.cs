using Microsoft.EntityFrameworkCore;
using WebApi.Infrastructure.Components;
using WebApi.Infrastructure.Models.DTO;
using WebApi.Infrastructure.Models.Requests;
using WebApi.Infrastructure.Models.Storage;

namespace WebApi.Services;

public class AdminService(DataComponent component)
{
    public async Task<List<UserForAdminDto>> GetUsers()
    {
        var users = await component.Users
            .Include(x => x.Role)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var blockMap = await component.BlockUsers
            .Where(b => b.BlockedUntil > now)
            .Select(u => u.UserId)
            .ToListAsync();

        return users.Select(u => new UserForAdminDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            About = u.About,
            DateOfBirth = u.DateOfBirth,
            TimeZone = u.TimeZone,
            RoleName = u.Role.RoleName,
            IsBlocked = blockMap.Contains(u.Id)
        }).ToList();
    }

    public async Task<EditUser> GetUserToEdit(int userId)
    {
        if (!await component.Users.AnyAsync(u => u.Id == userId))
            throw new Exception("Пользователь с данным Id не найден.");

        var blockedUntil = await component.BlockUsers
            .Where(b => b.UserId == userId && b.BlockedUntil > DateTime.UtcNow)
            .Select(u => u.BlockedUntil)
            .FirstOrDefaultAsync();

        return await component.Users
            .Include(x => x.Role)
            .Select(u => new EditUser
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                About = u.About,
                DateOfBirth = u.DateOfBirth,
                TimeZone = u.TimeZone,
                RoleName = u.Role.RoleName,
                Password = u.Password,
                BlockedUntil = blockedUntil
            })
            .FirstAsync(u => u.Id == userId);
    }

    public async Task<bool> EditUser(EditUser user)
    {
        var userEntry = await component.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (userEntry == null)
            throw new Exception("Пользователь с заданным Id не найден.");

        var roleEntry = await component.Roles
            .FirstOrDefaultAsync(r => r.RoleName == user.RoleName);

        if (roleEntry == null)
            throw new Exception($"Роль {user.RoleName} не найдена.");

        userEntry.FirstName = user.FirstName;
        userEntry.LastName = user.LastName;
        userEntry.Email = user.Email;
        userEntry.About = user.About;
        userEntry.DateOfBirth = DateTime.SpecifyKind(user.DateOfBirth, DateTimeKind.Utc);
        userEntry.TimeZone = user.TimeZone;
        userEntry.RoleId = roleEntry.Id;
        
        await UpdateBlockStatusAsync(user.Id, user.BlockedUntil);

        return await component.Update(userEntry);
    }

    private async Task UpdateBlockStatusAsync(int userId, DateTime? blockedUntil)
    {
        var existingBlock = await component.BlockUsers
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (blockedUntil == null || blockedUntil < DateTime.UtcNow)
        {
            if (existingBlock != null)
                await component.Delete<BlockUser>(existingBlock.Id);
        }
        else
        {
            if (existingBlock == null)
            {
                var newBlock = new BlockUser
                {
                    UserId = userId,
                    BlockedUntil = DateTime.SpecifyKind(blockedUntil.Value, DateTimeKind.Utc)
                };
                await component.Insert(newBlock);
            }
            else
            {
                existingBlock.BlockedUntil = blockedUntil.Value;
                await component.Update(existingBlock);
            }
        }
    }

/*
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