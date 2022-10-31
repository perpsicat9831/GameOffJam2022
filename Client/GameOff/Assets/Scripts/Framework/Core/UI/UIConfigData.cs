using System;

namespace Framework
{

    public class UIConfigData 
    {
        public readonly Type script;
        public readonly string prefabPath;

        public UIConfigData(Type _script,string _prefabPath)
        {
            script = _script;
            prefabPath = _prefabPath;
        }
    }
}
