using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace laster40Net.Util
{
    public static class Tick    
    {
        public static int Gap( int start, int end )
        {
            start &= int.MaxValue;
            end &= int.MaxValue;
            return ( ( ( end ) >= ( start ) ) ? ( ( end ) - ( start ) ) : ( int.MaxValue - ( start ) + ( end ) + 1) );
        }
    }

    /// <summary>
    /// 원자성을 지닌 Int값
    /// </summary>
    public class AtomicInt
    {
        private int _value = 0;
        public int Value { get { return _value; } set { Interlocked.Exchange(ref _value, value); } }

        /// <summary>
        /// 생성자(초기값 0)
        /// </summary>
        public AtomicInt()
        {
            _value = 0;
        }

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="initValue">초기값</param>
        public AtomicInt(int initValue)
        {
            _value = initValue;
        }

        /// <summary>
        /// CAS on
        /// </summary>
        /// <returns></returns>
        public bool CasOn()
        {
            return Interlocked.CompareExchange(ref _value, 1, 0) == 0;
        }

        /// <summary>
        /// CAS off
        /// </summary>
        /// <returns></returns>
        public bool CasOff()
        {
            return Interlocked.CompareExchange(ref _value, 0, 1) == 1;
        }

        public void On()
        {
            Interlocked.Exchange(ref _value, 1);
        }

        public void Off()
        {
            Interlocked.Exchange(ref _value, 0);
        }

        public bool IsOn()
        {
            return _value == 1;
        }

        public bool Test(int value)
        {
            return _value == value;
        }
    }

}
