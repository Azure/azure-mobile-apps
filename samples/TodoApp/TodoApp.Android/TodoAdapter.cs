// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;
using TodoApp.Data.Models;

namespace TodoApp.Android
{
    public class TodoAdapter : RecyclerView.Adapter
    {
        private readonly ITodoAdapterCallback _callback;
        private readonly List<TodoItem> itemList = new List<TodoItem>();

        public TodoAdapter(ITodoAdapterCallback callbackHandler)
        {
            _callback = callbackHandler;
        }

        public void AddItem(TodoItem item)
        {
            itemList.Add(item);
            NotifyItemInserted(itemList.IndexOf(item));
        }

        public void ReplaceItem(TodoItem item)
        {
            var index = itemList.FindIndex(t => t.Id == item.Id);
            if (index >= 0)
            {
                itemList[index] = item;
                NotifyItemChanged(index);
            }
        }

        public void RefreshItems(IEnumerable<TodoItem> items)
        {
            itemList.Clear();
            itemList.AddRange(items);
            NotifyDataSetChanged();
        }

        #region RecyclerView.Adapter
        public override int ItemCount => itemList.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            (holder as TodoItemViewHolder)?.BindItem(itemList[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row_list_item, parent, false);
            return new TodoItemViewHolder(layout, _callback);
        }
        #endregion
    }

    public class TodoItemViewHolder : RecyclerView.ViewHolder, CompoundButton.IOnCheckedChangeListener
    {
        private readonly CheckBox _checkbox;
        private readonly ITodoAdapterCallback _callback;
        private TodoItem _item;

        public TodoItemViewHolder(View itemView, ITodoAdapterCallback callback) : base(itemView)
        {
            _checkbox = itemView.FindViewById<CheckBox>(Resource.Id.check_todo_item);
            _callback = callback;
        }

        public void BindItem(TodoItem item)
        {
            _item = item;
            _checkbox.Text = item.Title;
            _checkbox.Checked = item.IsComplete;
            _checkbox.SetOnCheckedChangeListener(this);
        }

        public async void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
        {
            if (_item != null)
            {
                await _callback.UpdateItemFromListAsync(_item, isChecked);
            }
        }
    }
}