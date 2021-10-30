using System.Threading.Tasks;
using System.Collections.Generic;

namespace Octopus.BlazorSample
{
    public interface INode<T>
    {
        T Value { get; }
        IList<INode<T>> Children { get; }
        void AddChild(INode<T> child);
        void RemoveChild(INode<T> child);
        void ClearAllChildren();
    }

    public class TreeNode<T> : INode<T>
    {
        public TreeNode(T value)
        {
            this.Value = value;
            this.Children = new List<INode<T>>();
        }

        public T Value { get; set; }

        public IList<INode<T>> Children { get; private set; }

        public void AddChild(INode<T> child)
        {
            if (Value != null)
            {
                this.Children.Add(child);
            }
        }

        public void ClearAllChildren()
        {
            this.Children.Clear();
        }

        public void RemoveChild(INode<T> child)
        {
            if (Value != null)
            {

            }
        }
    }
}