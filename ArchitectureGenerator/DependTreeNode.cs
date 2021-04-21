using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchitectureGenerator
{
    class DependTreeNode<T> : IEqualityComparer<DependTreeNode<T>>
    {
        T Value { set; get; }
        public List<DependTreeNode<T>> Nodes { get; } = new List<DependTreeNode<T>>();
        bool IEqualityComparer<DependTreeNode<T>>.Equals(DependTreeNode<T> x, DependTreeNode<T> y)
        {
            return true;
        }

        int IEqualityComparer<DependTreeNode<T>>.GetHashCode(DependTreeNode<T> obj)
        {
            return 1;
        }
    }
}
