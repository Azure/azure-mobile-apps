using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Android.Widget.CompoundButton;

namespace ZumoQuickstart
{
    public class TodoAdapter : RecyclerView.Adapter
    {
        private ITodoAdapterCallback mCallbackHandler;
        private List<TodoItem> itemList;

        public override int ItemCount => itemList.Count;

        public TodoAdapter(ITodoAdapterCallback callbackHandler)
        {
            mCallbackHandler = callbackHandler;
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            (holder as TodoItemViewHolder).BindItem(itemList[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var layout = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.row_list_item, parent, false);
            return new TodoItemViewHolder(layout, mCallbackHandler);
        }
    }

    public class TodoItemViewHolder : RecyclerView.ViewHolder, IOnCheckedChangeListener
    {
        private CheckBox mCheckbox;
        private TodoItem mItem;
        private ITodoAdapterCallback mCallback;

        public TodoItemViewHolder(View itemView, ITodoAdapterCallback callback) : base(itemView)
        {
            mCheckbox = itemView.FindViewById<CheckBox>(Resource.Id.check_todo_item);
            mCallback = callback;
        }

        public void BindItem(TodoItem item)
        {
            mCheckbox.Text = item.Text;
            mCheckbox.Checked = item.Complete;
            mCheckbox.SetOnCheckedChangeListener(this);
        }

        public async void OnCheckedChanged(CompoundButton buttonView, bool isChecked)
        {
            if (mItem != null && mCallback != null)
            {
                await mCallback.UpdateItemFromListAsync(mItem, isChecked);
            }
        }
    }

    /// <summary>
    /// Definition of the callbacks needed to run the todo list adapter.
    /// </summary>
    public interface ITodoAdapterCallback
    {
        Task UpdateItemFromListAsync(TodoItem item, bool isChecked);
    }
}