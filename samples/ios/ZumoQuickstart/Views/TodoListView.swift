import SwiftUI

struct TodoListView: View {
    @State var todoItems: [TodoItem] = [
        TodoItem(text: "Item 1"),
        TodoItem(text: "Item 2"),
        TodoItem(text: "Item 3")
    ]
    
    func addNewItem(_ text: String) {
        if (!text.isEmpty) {
            self.todoItems.append(TodoItem(text: text))
        }
    }
    
    var body: some View {
        VStack {
            Header()
            List(todoItems) { todoItem in
                Text(todoItem.text)
            }
            Spacer()
            AddItemControl() { text in addNewItem(text) }
        }
    }
}

#if DEBUG
struct TodoListView_Previews: PreviewProvider {
    static var previews: some View {
        TodoListView()
    }
}
#endif
