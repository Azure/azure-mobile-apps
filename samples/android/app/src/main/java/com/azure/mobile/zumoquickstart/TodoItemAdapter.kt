package com.azure.mobile.zumoquickstart

import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.CheckBox
import androidx.recyclerview.widget.DiffUtil
import androidx.recyclerview.widget.ListAdapter
import androidx.recyclerview.widget.RecyclerView

class TodoItemAdapter(val onChangeListener: (TodoItem, Boolean) -> Unit)
    : RecyclerView.Adapter<TodoItemAdapter.ViewHolder>()
{
    var itemsInList: MutableList<TodoItem> = mutableListOf()

    fun refreshItems(items: List<TodoItem>) {
        itemsInList.clear()
        itemsInList.addAll(items)
        notifyDataSetChanged()
    }

    fun addItem(item: TodoItem) {
        itemsInList.add(item)
        notifyItemInserted(itemsInList.indexOf(item))
    }

    fun replaceItem(item: TodoItem) {
        val index = itemsInList.indexOfFirst { i -> i.id == item.id }
        if (index >= 0) {
            itemsInList[index] = item
            notifyItemChanged(index)
        }
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): ViewHolder
        = ViewHolder(LayoutInflater.from(parent.context).inflate(R.layout.row_list_item, parent, false))

    override fun onBindViewHolder(holder: ViewHolder, position: Int)
        = holder.bind(itemsInList[position])

    override fun getItemCount(): Int
        = itemsInList.size

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