using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackingKit_Core
{
    public class GroupManager
    {
        private Stack<HashSet<string>> _groupStack = new Stack<HashSet<string>>();

        public void StartGroup(params string[] idents)
        {
            _groupStack.Push(new HashSet<string>(idents));
        }

        public void EndGroup()
        {
            if (_groupStack.Count > 0)
            {
                _groupStack.Pop();
            }
            else
            {
                throw new Exception("There is no Group's left to End.");
            }
        }

        public IEnumerable<string> GetCurrentGroupTags()
        {
            return _groupStack.SelectMany(set => set);
        }
    }

}
