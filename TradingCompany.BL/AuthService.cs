using TradingCompany.Core.DTOs;
using TradingCompany.Core.Entities;
using TradingCompany.DAL.Interfaces;

namespace TradingCompany.BL;

public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IUserSession userSession) : IAuthService
{
    public const int LoginMaxLength = 50;
    public const int PasswordMinLength = 6;
    public const int FullNameMaxLength = 100;
    public const int PhoneMaxLength = 20;
    public const int EmailMaxLength = 100;

    public async Task<bool> LoginAsync(string login, string password)
    {
        var user = await userRepository.GetByLoginAsync(login.Trim());
        if (user is null)
        {
            passwordHasher.Hash(password, Guid.Empty);
            return false;
        }

        if (!passwordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
        {
            return false;
        }

        userSession.SignIn(ToDto(user));
        return true;
    }

    public void Logout() => userSession.SignOut();

    public async Task<List<UserDto>> GetAllAsync() =>
        (await userRepository.GetAllAsync())
            .OrderBy(u => u.Login, StringComparer.CurrentCultureIgnoreCase)
            .Select(ToDto)
            .ToList();

    public async Task<int> CreateUserAsync(CreateUserDto user)
    {
        var login = user.Login.Trim();
        var errors = new List<string>();

        if (login.Length == 0)
            errors.Add("Логін обов'язковий.");
        else if (login.Length > LoginMaxLength)
            errors.Add($"Логін не може бути довшим за {LoginMaxLength} символів.");

        errors.AddRange(ValidatePassword(user.Password));

        if (string.IsNullOrWhiteSpace(user.FullName))
            errors.Add("ПІБ обов'язкове.");
        else if (user.FullName.Trim().Length > FullNameMaxLength)
            errors.Add($"ПІБ не може бути довшим за {FullNameMaxLength} символів.");

        if (user.PhoneNumber.Trim().Length > PhoneMaxLength)
            errors.Add($"Телефон не може бути довшим за {PhoneMaxLength} символів.");

        var email = user.Email.Trim();
        if (email.Length == 0 || !email.Contains('@'))
            errors.Add("Вкажіть коректний email.");
        else if (email.Length > EmailMaxLength)
            errors.Add($"Email не може бути довшим за {EmailMaxLength} символів.");

        if (errors.Count == 0)
        {
            var all = await userRepository.GetAllAsync();
            if (all.Any(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Логін \"{login}\" вже зайнятий.");
            if (all.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)))
                errors.Add($"Email \"{email}\" вже використовується.");
        }

        if (errors.Count > 0)
            throw new BusinessRuleException(errors);

        var salt = Guid.NewGuid();
        var entity = new User
        {
            Login = login,
            PasswordSalt = salt,
            PasswordHash = passwordHasher.Hash(user.Password, salt),
            Role = user.IsDeliveryManager ? UserRole.DeliveryManager : UserRole.Customer,
            FullName = user.FullName.Trim(),
            PhoneNumber = user.PhoneNumber.Trim(),
            Email = email
        };

        await userRepository.AddAsync(entity);
        return entity.Id;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
    {
        var errors = ValidatePassword(newPassword);
        if (errors.Count > 0)
            throw new BusinessRuleException(errors);

        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return false;

        user.PasswordSalt = Guid.NewGuid();
        user.PasswordHash = passwordHasher.Hash(newPassword, user.PasswordSalt);
        await userRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> DeleteAsync(int userId)
    {
        if (userSession.CurrentUser?.Id == userId)
            throw new BusinessRuleException("Не можна видалити користувача, який зараз увійшов.");

        if (await userRepository.GetByIdAsync(userId) is null)
            return false;

        await userRepository.DeleteAsync(userId);
        return true;
    }

    private static List<string> ValidatePassword(string password)
    {
        var errors = new List<string>();
        if (password.Length < PasswordMinLength)
            errors.Add($"Пароль має містити щонайменше {PasswordMinLength} символів.");
        return errors;
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Login = user.Login,
        FullName = user.FullName,
        PhoneNumber = user.PhoneNumber,
        Email = user.Email,
        IsDeliveryManager = user.Role == UserRole.DeliveryManager
    };
}
