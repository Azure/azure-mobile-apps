import Foundation
import Combine
import MicrosoftAzureMobile

class TodoService: ObservableObject {
    static let shared = TodoService() // Singleton
    
    @Published var items = [TodoItem]()
    @Published var isBusy = false
    @Published var hasError = false
    @Published var errorMessage = ""
    
    private let client: MSClient
    private let table: MSTable
    
    private init() {
        client = MSClient(applicationURLString: Constants.BackendURL)
        table = client.table(withName: "TodoItem")
        getTodoItems()
    }
    
    private func initialize(completion: (Error?) -> Void) {
        completion(nil)
    }
    
    private func setErrorCondition(_ error: Error) {
        self.hasError = true
        self.errorMessage = error.localizedDescription
        self.isBusy = false
    }
    
    func getTodoItems() -> Void {
        if (self.isBusy) { return }

        self.isBusy = true
        initialize { initError in
            if let initError = initError {
                self.setErrorCondition(initError)
            } else {
                table.query().read { (queryResult, readError) in
                    if let readError = readError {
                        self.setErrorCondition(readError)
                    } else {
                        // Convert the queryResult to the items and store
                        self.isBusy = false
                    }
                }
            }
        }
    }
    
    func addTodoItem(_ text: String) {
        initialize { initError in
            if let initError = initError {
                self.setErrorCondition(initError)
            } else {
                // Create item, convert to AnyHashable, store, then returned item to items
                items.append(TodoItem(text: text))
            }
        }
    }
    
    func toggleCompletion(_ item: TodoItem) {
        saveTodoItem(TodoItem(id: item.id, text: item.text, complete: !item.complete))
    }
    
    func saveTodoItem(_ item: TodoItem) {
        // Create item, convert to AnyHashable, store, then fix up items
        if let index = items.firstIndex(where: { $0.id == item.id }) {
            items[index] = item
        } else {
            hasError = true
            errorMessage = "Item \(item.id) is not found"
        }
    }
}


