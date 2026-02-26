using ChatApp.Backend.Services;

namespace ChatApp.Backend.Tests
{
    public class PasswordHasherTests
    {
        [Test]
        public void HashPassword_CreatesHashThatCanBeVerified()
        {
            var password = "P@ssw0rd!";

            var hash = PasswordHasher.HashPassword(password);

            Assert.That(hash, Is.Not.EqualTo(password));
            Assert.That(PasswordHasher.VerifyPassword(password, hash), Is.True);
        }

        [Test]
        public void VerifyPassword_ReturnsFalseForWrongPassword()
        {
            var hash = PasswordHasher.HashPassword("correct-password");

            var isValid = PasswordHasher.VerifyPassword("wrong-password", hash);

            Assert.That(isValid, Is.False);
        }
    }
}
