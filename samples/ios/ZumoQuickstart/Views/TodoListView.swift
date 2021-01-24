import SwiftUI

struct TodoListView: View {
    @ObservedObject var repository = TodoService.shared
    
    var body: some View {
        VStack {
            Header(onRefresh: { repository.getTodoItems() })
            if (repository.isBusy) {
                ActivityIndicator(shouldAnimate: $repository.isBusy)
            }
            List(repository.items) { todoItem in
                HStack {
                    CheckItem(todoItem.complete, onChange: { repository.toggleCompletion(todoItem) })
                        .disabled(repository.isBusy)
                    Text(todoItem.text)
                }
            }
            Spacer()
            AddItemControl() { text in
                repository.addTodoItem(text)
            }.disabled(repository.isBusy)
        }.alert(
            isPresented: $repository.hasError,
            content: { Alert(title: Text(repository.errorMessage)) }
        )
    }
}

#if DEBUG
struct TodoListView_Previews: PreviewProvider {
    static var previews: some View {
        TodoListView()
    }
}
#endif
