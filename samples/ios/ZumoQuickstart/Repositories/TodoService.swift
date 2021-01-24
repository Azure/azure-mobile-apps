import Foundation
import Combine

enum RepositoryError: Error {
    case itemNotFound(id: String?)
}

class TodoService: ObservableObject {
    static let shared = TodoService() // Singleton
    
    @Published var items = [TodoItem]()
    @Published var isBusy = false
    
    private init() {
        items = [
            TodoItem(text: "Do the tutorial"),
            TodoItem(text: "Do more work"),
            TodoItem(text: "Publish an app")
        ]
    }
    
    func addTodoItem(_ text: String, completion: (Error?) -> Void) {
        let newItem = TodoItem(text: text)
        items.append(newItem)
        completion(nil)
    }
    
    func saveTodoItem(_ item: TodoItem, completion: (Error?) -> Void) {
        if let index = items.firstIndex(where: { $0.id == item.id }) {
            items[index] = item
            completion(nil)
        }
        completion(RepositoryError.itemNotFound(id: item.id))
    }
}


