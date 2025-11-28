using System.Security.Cryptography;
using System.Text;

namespace P3AHR.Extensions
{
    public class OtherServices
    {
        public bool Check(object model)
        {

            if (model != null)
            {
                if (model.GetType() == typeof(byte[]))
                {
                    try
                    {
                        byte[] newLogo = Convert.FromBase64String(model.ToString());
                        if (newLogo.Length > 0 && newLogo != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                if (!string.IsNullOrEmpty(model.ToString()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
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
