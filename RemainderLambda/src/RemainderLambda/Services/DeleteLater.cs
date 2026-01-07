using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemainderLambda.Services
{
    internal class DeleteLater
    {
    }

    public class Animal
    {
        // TODO: Declare a virtual method MakeSound
        public virtual void MakeSound()
        {
            Console.WriteLine("Animal sound");
        }
    }

    public class Dog : Animal
    {
        // TODO: Override the MakeSound method
        public void MakeSound()
        {
            Console.WriteLine("Dog barks");
        }
    }
}
