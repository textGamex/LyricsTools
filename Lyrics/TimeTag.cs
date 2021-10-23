using System;
using System.Collections.Generic;

namespace Lyrics
{
    public struct TimeTag : IComparable<TimeTag>, IComparer<TimeTag>
    {
        public readonly static TimeTag Zero = new TimeTag(0, 0, 0);
        /// <summary>
        /// 分钟最大值
        /// </summary>
        public readonly static byte MINUTE_MAX = 59;
        /// <summary>
        /// 秒最大值
        /// </summary>
        public readonly static byte SECOND_MAX = 59;
        /// <summary>
        /// 毫秒最大值
        /// </summary>
        public static readonly ushort MILLISECOND_MAX = 999;
        /// <summary>
        /// 分钟
        /// </summary>
        private readonly byte minute;
        /// <summary>
        /// 秒
        /// </summary>
        private readonly byte second;
        /// <summary>
        /// 毫秒
        /// </summary>
        private readonly ushort millisecond;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newMinute">分</param>
        /// <param name="newSecond">秒</param>
        /// <param name="newMillisecond">毫秒</param>
        /// <exception cref="ArgumentException"></exception>
        public TimeTag(uint newMinute, uint newSecond, uint newMillisecond)
        {
            if (newMinute > MINUTE_MAX)
            {
                throw new ArgumentException($"{newMinute} > {MINUTE_MAX}");
            }
            if (newSecond > SECOND_MAX)
            {
                throw new ArgumentException($"{newSecond} > {SECOND_MAX}");
            }
            if (newMillisecond > MILLISECOND_MAX)
            {
                throw new ArgumentException($"{newMillisecond} > {MILLISECOND_MAX}");
            }
            minute = (byte)newMinute;
            second = (byte)newSecond;
            millisecond = (ushort)newMillisecond;
        }

        /// <summary>
        /// 使用字符串构建一个时间标签
        /// </summary>
        /// <param name="line">字符串形式的时间标签</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TimeTag(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            //去除歌词和[]
            string timeTag = line.Split(new char[] {'[', ']'})[1];
            string[] timeArray = timeTag.Split(new char[] {':', '.'});

            minute = byte.Parse(timeArray[0]);
            second = byte.Parse(timeArray[1]);
            string millisecondString = timeArray[2];            
            millisecond = ushort.Parse(millisecondString);
            if (millisecondString.Length == 2)
                millisecond *= 10;
        }

        /// <summary>
        /// 分钟
        /// </summary>
        public uint Minute 
        {
            get => minute;
        }

        /// <summary>
        /// 秒
        /// </summary>
        public uint Second
        {
            get => second;
        }

        /// <summary>
        /// 毫秒
        /// </summary>
        public uint Millisecond
        {
            get => millisecond;
        }

        /// <summary>
        /// 返回一个标准的时间标签
        /// </summary>
        /// <returns>一个标准的时间标签</returns>
        public string ToTimeTag()
        {
            return string.Format("[{0:d2}:{1:d2}.{2:d3}]", Minute, Second, Millisecond);
        }

        /// <summary>
        /// 返回基于this加上毫秒的副本
        /// </summary>
        /// <param name="addMillisecond">增加的毫秒</param>
        /// <returns>基于this加上毫秒的副本</returns>
        /// <exception cref="ArgumentException">如果时间过大</exception>
        /// <date>2021-10-1</date>
        public TimeTag PlusMillisecond(uint addMillisecond)
        {
            uint newMillisecond = Millisecond + addMillisecond;
            uint newMinute = Minute;
            uint newSecond = Second;

            while (newMillisecond > MILLISECOND_MAX)
            {
                newMillisecond -= 1000;
                ++newSecond;
                if (newSecond > SECOND_MAX)
                {
                    ++newMinute;
                    newSecond -= 60;
                }
            }
            if (newMinute > MINUTE_MAX)
            {
                throw new ArgumentException($"{newMinute} > {MINUTE_MAX}");
            }
            return new TimeTag(newMinute, newSecond, newMillisecond);
        }

        public override string ToString()
        {
            return string.Format("{{分={0:d2} 秒={1:d2} 毫秒={2:d3} }}", Minute, Second, Millisecond);            
        }

        /// <summary>
        /// 返回总和的毫秒
        /// </summary>
        /// <returns>换算成毫秒的数</returns>
        public uint ToMillisecond()
        {
            return (Minute * 60 + Second) * 1000 + Millisecond;
        }
        #region 杂
        public override bool Equals(object obj)
        {
            return obj is TimeTag tag &&
                   minute == tag.minute &&
                   second == tag.second &&
                   millisecond == tag.millisecond;
        }

        public override int GetHashCode()
        {
            int hashCode = -1864587320;
            hashCode = hashCode * -1521134295 + minute.GetHashCode();
            hashCode = hashCode * -1521134295 + second.GetHashCode();
            hashCode = hashCode * -1521134295 + millisecond.GetHashCode();
            return hashCode;
        }

        public int Compare(TimeTag x, TimeTag y)
        {
            if (x == y)
            {
                return 0;
            }

            return x.ToMillisecond().CompareTo(y.ToMillisecond());           
        }

        public int CompareTo(TimeTag x)
        {
            return Compare(this, x);
        }
        #endregion

        #region 运算符重载

        public static TimeTag operator +(in TimeTag left, in TimeTag right)
        {
            return left.PlusMillisecond(right.ToMillisecond());
        }

        public static bool operator ==(in TimeTag left, in TimeTag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in TimeTag left, in TimeTag right)
        {
            return !(left == right);
        }

        public static bool operator >(in TimeTag left, in TimeTag right)
        {            
            if (left.CompareTo(right) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator <(in TimeTag left, in TimeTag right)
        {
            if (left == right)
            {
                return false;
            }
            return !(left > right);
        }

        public static bool operator >=(in TimeTag left, in TimeTag right)
        {
            return !(left < right);
        }

        public static bool operator <=(in TimeTag left, in TimeTag right)
        {
            return !(left > right);
        }
        #endregion
    }
}
