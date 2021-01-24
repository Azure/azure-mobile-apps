import SwiftUI

struct TodoListView: View {
    @ObservedObject var repository = TodoService.shared
    @State var isAlerting = false
    @State var alertText = ""
    
    var body: some View {
        VStack {
            Header()
            List(repository.items) { todoItem in
                Text(todoItem.text)
            }
            Spacer()
            AddItemControl() { text in
                repository.addTodoItem(text) { error in
                    if error != nil {
                        self.isAlerting = true
                    }
                }
            }
        }.alert(isPresented: $isAlerting, content: {
            Alert(title: Text("Error"))
        })
    }
}

#if DEBUG
struct TodoListView_Previews: PreviewProvider {
    static var previews: some View {
        TodoListView()
    }
}
#endif
