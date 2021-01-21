import SwiftUI

/**!
    Displays the UI for a single entry in the list of items.
 
    - Parameter: item {TodoItem} the item to be displayed
    - Parameter: action {() -> Void} called when the user presses the item
 */
struct TodoListCell: View {
    var item: TodoItem
    let action: ((_ item: TodoItem) -> Void)?
    let icon: String
    
    @State var text: String = ""
    @State var complete: Bool = false
    
    /**!
        Initialization - will copy the item and action, then compute the
         state variables.
     */
    init(_ item: TodoItem, action: ((_ item: TodoItem) -> Void)? = nil) {
        self.item = item
        self.action = action
        self.icon = self.item.complete ? "checkmark.circle.fill" : "circle"
        self.text = self.item.text
        self.complete = self.item.complete
    }
    
    /**!
        Triggers the action, if it is specified.
     */
    func triggerAction() {
        if let action = self.action {
            let newItem = TodoItem(id: self.item.id, text: self.text, complete: self.complete)
            action(newItem)
        }
    }
    
    /**!
        The UI for the TodoListCell
     */
    var body: some View {
        HStack {
            // The checkbox icon
            Image(systemName: self.icon)
                .resizable()
                .frame(width: 20, height: 20)
                .onTapGesture {
                    complete.toggle()
                    self.triggerAction()
                }
            TextField("Enter some text", text: $text, onCommit: { self.triggerAction() })
        }
    }
}

#if DEBUG
struct TodoListCell_Previews: PreviewProvider {
    static var completeItem = TodoItem.newTodoItem("Completed Item", complete: true)
    static var incompleteItem = TodoItem.newTodoItem("Incomplete Item", complete: false)
    
    static var previews: some View {
        VStack {
            TodoListCell(completeItem)
                .padding(2)
                .border(Color.black, width: 1)
            TodoListCell(incompleteItem)
                .padding(2)
                .border(Color.red, width: 1)
        }.padding(6)
    }
}
#endif
