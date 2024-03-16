using System;
using System.Linq;
using System.Reflection;

namespace FastSocketLite.SocketBase.Utils
{
    public static class ReflectionHelper
    {
        /// <summary>
        /// 지정된 클래스 타입을 구현하는 기본 클래스 인스턴스를 가져온다.
        /// </summary>
        /// <typeparam name="T">인터페이스 유형</typeparam>
        /// <param name="assembly">지정된 어셈블리</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">assembly is null</exception>
        static public T[] GetImplementObjects<T>(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");

            return assembly.GetExportedTypes().Where(c =>
            {
                if (c.IsClass && !c.IsAbstract)
                {
                    var interfaces = c.GetInterfaces();
                    if (interfaces != null) return interfaces.Contains(typeof(T));
                }
                return false;
            }).Select(c => (T)c.GetConstructor(new Type[0]).Invoke(new object[0])).ToArray();
        }
    }
}