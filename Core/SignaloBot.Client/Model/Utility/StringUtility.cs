using SignaloBot.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace SignaloBot.Client
{
    public class StringUtility
    {
        /// <summary>
        /// Сократить размер подстроки, чтобы вмести в маскимальный размер, учитывая длину имеющейся статичной части.
        /// Максимальный размер берётся равным предельной длине заголовка Email сообщения DALConstants.EMAIL_MAX_SUBJECT_LENGTH = 200.
        /// </summary>
        /// <param name="partString">Подстрока, которую необходимо сократить</param>
        /// <param name="fixedLength">Длина статичной части заголовка</param>
        /// <param name="maxLength">Максимальная длина итоговой строки</param>
        /// <param name="stringRepeatTimes">Число повторов подстроки</param>
        /// <returns></returns>
        public static string ShortenSubjectString(string partString, int fixedLength
            , int stringRepeatTimes = 1)
        {
            return ShortenSubjectString(partString, fixedLength 
                , DALConstants.EMAIL_MAX_SUBJECT_LENGTH, stringRepeatTimes);
        }

        /// <summary>
        /// Сократить размер подстроки, чтобы вмести в маскимальный размер, учитывая длину имеющейся статичной части.
        /// </summary>
        /// <param name="partString">Подстрока, которую необходимо сократить</param>
        /// <param name="fixedLength">Длина статичной части заголовка</param>
        /// <param name="maxLength">Максимальная длина итоговой строки</param>
        /// <param name="stringRepeatTimes">Число повторов подстроки</param>
        /// <returns></returns>
        public static string ShortenSubjectString(string partString, int fixedLength, int maxLength
            , int stringRepeatTimes = 1)
        {
            if (stringRepeatTimes < 1)
                throw new Exception("Число повторов строки не может быть меньше 1.");

            int maxLengthForAllParts = (maxLength - fixedLength);
            int maxLengthForEachPart = (int)Math.Floor(maxLengthForAllParts / (decimal)stringRepeatTimes);
            
            if (partString.Length > maxLengthForEachPart)
            {
                string shortSuffix = "...";

                int newlength = maxLengthForEachPart - shortSuffix.Length;
                if (newlength < 0)
                    newlength = 0;

                return partString.Substring(0, newlength) + shortSuffix;
            }
            else
            {
                return partString;
            }
        }
    }
}