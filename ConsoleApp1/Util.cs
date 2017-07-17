using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MarketingPlatForm.Client
{
    class Util
    {
        public static string GetImageBase64String(string imagePath)
        {
            var result = "";

            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                result = Convert.ToBase64String(data);
            }

            return result;
        }

        //public static string GetIndustryDescription(TreeNode[] industryCategories, int count)
        //{
        //    var builder = new StringBuilder();

        //    for (int i = 0; i < count; i++)
        //    {
        //        var node = industryCategories[i];
        //        builder.Append(node.Name + " > ");
        //    }

        //    if (builder.Length > 0)
        //    {
        //        builder.Remove(builder.Length - 3, 3);
        //    }

        //    return builder.ToString();
        //}

        //public static string GetIndustryDescriptionReverse(TreeNode[] industryCategories, int count)
        //{
        //    var builder = new StringBuilder();

        //    for (int i = 0; i < count; i++)
        //    {
        //        var node = industryCategories[i];
        //        builder.Insert(0, node.Name + " > ");
        //    }

        //    if (builder.Length > 0)
        //    {
        //        builder.Remove(builder.Length - 3, 3);
        //    }

        //    return builder.ToString();
        //}

        public static bool CheckMobile(string userName, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(userName))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入手机号码！";
                    return false;
                }
            }

            if (!Regex.IsMatch(userName, @"0?(134|135|136|137|138|139|147|150|151|152|157|158|159|172|178|182|183|184|187|188|130|131|132|145|155|156|171|175|176|185|186|133|149|153|173|177|180|181|189|170)[0-9]{8}"))
            {
                message = "格式不正确，请输入手机号码！";
                return false;
            }

            return true;
        }

        public static bool CheckPassword(string password, bool checkLength, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrEmpty(password))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入密码！";
                    return false;
                }
            }

            if (checkLength && (password.Length < 6 || password.Length > 12))
            {
                message = "密码长度为 6-12 位！";
                return false;
            }

            return true;
        }

        public static bool CheckAreaCode(string code, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(code))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入区号！";
                    return false;
                }
            }
            if (!Regex.IsMatch(code, @"^\d{3,4}$"))
            {
                message = "格式不正确，请输入区号！";
                return false;
            }

            return true;
        }

        public static bool CheckTel(string tel, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(tel))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入电话号码！";
                    return false;
                }
            }
            if (!Regex.IsMatch(tel, @"^\d{7,8}$"))
            {
                message = "格式不正确，请输入电话号码！";
                return false;
            }

            return true;
        }

        public static bool CheckExtension(string extension, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(extension))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入分机号！";
                    return false;
                }
            }
            if (!Regex.IsMatch(extension, @"^\d{1,5}$"))
            {
                message = "格式不正确，请输入分机号！";
                return false;
            }

            return true;
        }

        public static bool CheckQQ(string qq, bool allowEmpty, out string message)
        {
            message = "";
            if (string.IsNullOrWhiteSpace(qq))
            {
                if (allowEmpty)
                {
                    return true;
                }
                else
                {
                    message = "请输入QQ号！";
                    return false;
                }
            }
            if (!Regex.IsMatch(qq, @"^[1-9]\d{4,14}$"))
            {
                message = "格式不正确，请输入QQ号！";
                return false;
            }

            return true;
        }


    }
}
