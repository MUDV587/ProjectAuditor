using System;
using UnityEditor.IMGUI.Controls;

namespace Unity.ProjectAuditor.Editor.UI.Framework
{
    internal struct TreeItemIdentifier
    {
        internal string nameWithIndex { get; private set; }

        internal string name { get; private set; }

        // stephenm TODO - Pretty sure this can go. Assemblies don't have indeces. I think the most we'll need is a flag
        // to say whether this is the "All" TreeItemIdentifier (i.e. (nameWithIndex == "All"))
        internal int index { get; private set; }

        internal static int kAll = -1;
        internal static int kSingle = 0;

        internal TreeItemIdentifier(string _name, int _index)
        {
            name = _name;
            index = _index;
            if (index == kAll)
                nameWithIndex = string.Format("All:{1}", index, name);
            else
                nameWithIndex = string.Format("{0}:{1}", index, name);
        }

        internal TreeItemIdentifier(TreeItemIdentifier treeItemIdentifier)
        {
            name = treeItemIdentifier.name;
            index = treeItemIdentifier.index;
            nameWithIndex = treeItemIdentifier.nameWithIndex;
        }

        internal TreeItemIdentifier(string _nameWithIndex)
        {
            // stephenm TODO - Pretty sure this can go. Assembly names don't have a foo:N (or N:foo?) naming convention like threads do.
            // So index should probably always be treated as 0 (sorry, "kSingle")
            nameWithIndex = _nameWithIndex;

            var tokens = nameWithIndex.Split(':');
            if (tokens.Length >= 2)
            {
                name = tokens[1];
                var indexString = tokens[0];
                if (indexString == "All")
                {
                    index = kAll;
                }
                else
                {
                    int intValue;
                    if (int.TryParse(tokens[0], out intValue))
                        index = intValue;
                    else
                        index = kSingle;
                }
            }
            else
            {
                index = kSingle;
                name = nameWithIndex;
            }
        }

        void UpdateAssemblyNameWithIndex()
        {
            if (index == kAll)
                nameWithIndex = string.Format("All:{1}", index, name);
            else
                nameWithIndex = string.Format("{0}:{1}", index, name);
        }

        internal void SetName(string newName)
        {
            name = newName;
            UpdateAssemblyNameWithIndex();
        }

        internal void SetIndex(int newIndex)
        {
            index = newIndex;
            UpdateAssemblyNameWithIndex();
        }

        internal void SetAll()
        {
            SetIndex(kAll);
        }
    }

    class SelectionWindowTreeViewItem : TreeViewItem
    {
        internal readonly TreeItemIdentifier TreeItemIdentifier;

        internal SelectionWindowTreeViewItem(int id, int depth, string displayName, TreeItemIdentifier treeItemIdentifier)
            : base(id, depth, displayName)
        {
            TreeItemIdentifier = treeItemIdentifier;
        }
    }
}
