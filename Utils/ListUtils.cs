using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BagOfTricks.Utils {
    public static class ListUtils {
        public static void MoveElementDown<T>(ref List<T> list, T element) {
            int index = list.IndexOf(element);

            if (index < list.Count - 1) {
                T swappedElement = list[index + 1];

                list[index + 1] = element;
                list[index] = swappedElement;
            }
        }
        public static void MoveElementUp<T>(ref List<T> list, T element) {
            int index = list.IndexOf(element);

            if (index > 0) {
                T swappedElement = list[index - 1];

                list[index - 1] = element;
                list[index] = swappedElement;
            }
        }

        public static void MakeElementLast<T>(ref List<T> list, T element) {
            int index = list.IndexOf(element);

            if (index < list.Count - 1) {
                list.Remove(element);
                list.Add(element);
            }
        }
        public static void MakeElementFirst<T>(ref List<T> list, T element) {
            int index = list.IndexOf(element);

            if (index > 0) {
                list.Remove(element);
                list.Insert(0, element);
            }
        }
    }
}
