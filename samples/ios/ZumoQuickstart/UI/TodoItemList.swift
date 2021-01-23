import SwiftUI

struct TodoItemList: View {
    var todoItems: [TodoItem] = [
        TodoItem(text: "Item 1"),
        TodoItem(text: "Item 2"),
        TodoItem(text: "Item 3")
    ]
    
    var body: some View {
        List(todoItems) { todoItem in
            Text(todoItem.text)
        }
    }
}

struct TodoItemList_Previews: PreviewProvider {
    static var previews: some View {
        TodoItemList()
    }
}
