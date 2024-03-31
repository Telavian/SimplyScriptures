namespace SimplyScriptures.Common.Extensions;

public static class NodeExtensions
{
    public static IEnumerable<TNode> Traverse<TNode>(this TNode node, Func<TNode, IEnumerable<TNode>> childrenSelector)
    {
        var stack = new Stack<TNode>();
        stack.Push(node);

        return TraverseNodes(stack, childrenSelector);
    }

    public static IEnumerable<TNode> TraverseItems<TNode>(this IEnumerable<TNode> nodes, Func<TNode, IEnumerable<TNode>> childrenSelector)
    {
        var stack = new Stack<TNode>();

        foreach (var node in nodes.Reverse())
        {
            stack.Push(node);
        }

        return TraverseNodes(stack, childrenSelector);
    }

    #region Private Methods

    private static IEnumerable<TNode> TraverseNodes<TNode>(Stack<TNode> nodes, Func<TNode, IEnumerable<TNode>> childrenSelector)
    {
        while (nodes.Count > 0)
        {
            var current = nodes.Pop();
            yield return current;

            var children = childrenSelector(current);
            foreach (var child in children.Reverse())
            {
                nodes.Push(child);
            }
        }
    }

    #endregion
}