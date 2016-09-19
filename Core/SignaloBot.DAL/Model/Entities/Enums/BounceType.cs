using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignaloBot.DAL
{
    /// <summary>
    /// Тип отказа в доставке сообщения.
    /// </summary>
    public enum BounceType
    {
        /// <summary>
        /// Больше не использовать этого получателя в рассылке.
        /// </summary>
        HardBounce,

        /// <summary>
        /// Возможна повторная попытка рассылки на этот адрес.
        /// </summary>
        SoftBounce,

        /// <summary>
        /// Неизвестный тип.
        /// </summary>
        Unknown
    }
}
