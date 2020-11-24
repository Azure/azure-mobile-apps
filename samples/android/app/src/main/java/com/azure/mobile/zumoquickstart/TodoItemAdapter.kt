package com.azure.mobile.zumoquickstart

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.CheckBox
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView

class TodoItemAdapter(val onChangeListener: (TodoItem, Boolean) -> Unit)
    : ListAdapter<TodoItem, TodoItemAdapter.ViewHolder>(DiffCallback())
{
    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): TodoItemAdapter.ViewHolder
        = ViewHolder(LayoutInflater.from(parent.context).inflate(R.layout.row_list_item, parent, false))

    override fun onBindViewHolder(holder: TodoItemAdapter.ViewHolder, position: Int) {
        holder.bind(getItem(position))
    }

    /**
     * Part of the ListAdapter specification - this class helps with determining if the items
     * are the same or not.
     */
    private class DiffCallback: DiffUtil.ItemCallback<TodoItem>()
    {
        override fun areItemsTheSame(oldItem: TodoItem, newItem: TodoItem): Boolean
            = oldItem.id == newItem.id

        override fun areContentsTheSame(oldItem: TodoItem, newItem: TodoItem): Boolean
            = (oldItem.text == newItem.text) && (oldItem.complete == newItem.complete)
    }

    /**
     * Binding for each row.
     */
    inner class ViewHolder(private val containerView: View) : RecyclerView.ViewHolder(containerView)
    {
        fun bind(item: TodoItem) {
            val checkbox = containerView.findViewById<CheckBox>(R.id.check_todo_item)

            checkbox.text = item.text
            checkbox.isChecked = item.complete
            checkbox.setOnCheckedChangeListener { _, isChecked ->
                onChangeListener?.invoke(item, isChecked)
            }
        }
    }
}