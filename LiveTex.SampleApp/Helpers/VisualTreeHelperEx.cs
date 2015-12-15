using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace LiveTex.SampleApp.Helpers
{
	internal static class VisualTreeHelperEx
	{
		public static T GetDescendant<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				return null;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();
				var descendant = element as T;
				if (descendant != null
					&& !ReferenceEquals(descendant, dependencyObject))
				{
					return descendant;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}

			return null;
		}

		public static IEnumerable<T> GetDescendants<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				yield break;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();

				var descendant = element as T;
				if (descendant != null
					&& !ReferenceEquals(descendant, dependencyObject))
				{
					yield return descendant;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}
		}

		public static IEnumerable<T> GetSelfAndDescendants<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject == null)
			{
				yield break;
			}

			var queue = new Queue<DependencyObject>();
			queue.Enqueue(dependencyObject);

			while (queue.Count > 0)
			{
				var element = queue.Dequeue();

				var descendant = element as T;
				if (descendant != null)
				{
					yield return descendant;
				}

				for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
				{
					queue.Enqueue(VisualTreeHelper.GetChild(element, index));
				}
			}
		}

		public static T GetAncestor<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			var current = dependencyObject;

			while (current != null)
			{
				var ancestor = current as T;
				if (ancestor != null)
				{
					return ancestor;
				}

				current = VisualTreeHelper.GetParent(current);
			}

			return null;
		}

		public static IEnumerable<T> GetAncestors<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			var current = dependencyObject;

			while (current != null)
			{
				var ancestor = current as T;
				if (ancestor != null)
				{
					yield return ancestor;
				}

				current = VisualTreeHelper.GetParent(current);
			}
		}
	}
}
