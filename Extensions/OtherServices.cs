using System.Security.Cryptography;
using System.Text;

namespace EMO.Extensions
{
    public class OtherServices
    {
        public bool Check(object model)
        {
            if (model == null)
            {
                return false;
            }

            if (model is int intValue)
            {
                return intValue != 0;
            }

            if (model is byte[] byteArray)
            {
                return byteArray.Length > 0;
            }

            if (model is string str)
            {
                return !string.IsNullOrEmpty(str);
            }

            if (model is bool boolValue)
            {
                return boolValue;
            }

            if (model is float floatValue)
            {
                return floatValue != 0.0f;
            }

            if (model is double doubleValue)
            {
                return doubleValue != 0.0;
            }

            if (model is decimal decimalValue)
            {
                return decimalValue != 0.0m;
            }

            return false;
        }
        public Guid[] ConvertIntoGuidLists(List<string> model)
        {
            var x = new List<Guid>();
            foreach (var item in model)
            {
                x.Add(Guid.Parse(item));
            }
            return x.ToArray();
        }
        public Guid[] ConvertIntoGuidLists(string[] model)
        {
            var x = new List<Guid>();
            foreach (var item in model)
            {
                x.Add(Guid.Parse(item));
            }
            return x.ToArray();
        }
        public List<string> ConvertIntostringLists(Guid[] model)
        {
            var x = new List<string>();
            foreach (var item in model)
            {
                x.Add(item.ToString());
            }
            return x;
        }
        public List<string> ConvertIntostringLists(string[] model)
        {
            var x = new List<string>();
            foreach (var item in model)
            {
                x.Add(item.ToString());
            }
            return x;
        }
        public List<string> ConvertIntostringLists(List<Guid> model)
        {
            var x = new List<string>();
            foreach (var item in model)
            {
                x.Add(item.ToString());
            }
            return x;
        }
        public string encodePassword(string password)
        {
            string currentPassword = "";
            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] hashValue =
                mySHA256.ComputeHash(Encoding.UTF8.GetBytes(password));
                currentPassword = Convert.ToBase64String(hashValue);
            }
            return currentPassword;
        }
        public string GenerateIconFromTitle(string title)
        {
            var titleString = title.Split(' ');
            var icon = string.Empty;
            foreach (var x in titleString)
            {
                icon = icon + x.Substring(0, 1)[0];
            }
            return icon;
        }
    }
}
