using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureGenerator
{
    class DependTree<T>
    {
        DependTreeNode<T> HeadNode { get; } = new DependTreeNode<T>();
    }
}
