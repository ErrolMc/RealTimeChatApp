using System.Globalization;
using ChatApp.Shared.TableDataSimple;
using ChatApp.Shared.Tables;
using ChatAppFrontEnd.Source.Converters;
using ChatAppFrontEnd.Source.Other.Caching.Data;

namespace ChatAppFrontend.Tests
{
    public class UtilityTests
    {
        [Test]
        public void EmptyStringToVisibilityConverter_ReturnsTrueForEmptyString()
        {
            var converter = new EmptyStringToVisibilityConverter();

            var result = converter.Convert(string.Empty, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void EmptyStringToVisibilityConverter_ReturnsFalseForNonEmptyString()
        {
            var converter = new EmptyStringToVisibilityConverter();

            var result = converter.Convert("hello", typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void EmptyStringToVisibilityConverter_ReturnsFalseForNonStringValues()
        {
            var converter = new EmptyStringToVisibilityConverter();

            var result = converter.Convert(123, typeof(bool), null!, CultureInfo.InvariantCulture);

            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void EmptyStringToVisibilityConverter_ConvertBackThrowsNotImplementedException()
        {
            var converter = new EmptyStringToVisibilityConverter();

            Assert.Throws<NotImplementedException>(() =>
                converter.ConvertBack(true, typeof(string), null!, CultureInfo.InvariantCulture));
        }

        [Test]
        public void ToMessageCache_MapsMessageFields()
        {
            var fromUser = new UserSimple { UserID = "u1", UserName = "Alice" };
            var message = new Message
            {
                ID = "m1",
                ThreadID = "t1",
                FromUser = fromUser,
                MessageContents = "Hello",
                TimeStamp = 12345
            };

            var cache = message.ToMessageCache();

            Assert.That(cache.MessageID, Is.EqualTo("m1"));
            Assert.That(cache.ThreadID, Is.EqualTo("t1"));
            Assert.That(cache.FromUser, Is.SameAs(fromUser));
            Assert.That(cache.Message, Is.EqualTo("Hello"));
            Assert.That(cache.TimeStamp, Is.EqualTo(12345));
        }
    }
}
