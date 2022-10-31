using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class DefaultObjectFactory<T> : IObjectFactory<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }
}
