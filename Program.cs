using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleIoC
{    
    class Program
    {
        static void Main(string[] args)
        {
            var ex = new Exception();
            IoC.Register<INested3>(new Nested3()); //singleton
            IoC.Register<INested2>(IoC.Create<Nested2>); //auto resolve dependencies
            IoC.Register<INested1>(IoC.Create<Nested1>); //auto resolve dependencies

            IoC.Resolve<INested1>();
            Console.ReadKey();
        }
    }

    interface INested3
    {
    }

    class Nested3 : INested3
    {
        public Nested3()
        {
            Console.WriteLine("Nested3");
        }
    }

    interface INested2
    {
    }

    class Nested2 : INested2
    {
        public Nested2(INested3 n3)
        {
            Console.WriteLine("Nested2");
        }
    }

    interface INested1
    {
    }

    class Nested1 : INested1
    {
        public Nested1(INested2 n2)
        {
            Console.WriteLine("Nested1");
        }
    }
}
