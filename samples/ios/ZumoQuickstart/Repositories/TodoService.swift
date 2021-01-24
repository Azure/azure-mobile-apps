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
        self.errorMessage = error.localizedDescription
        self.hasError = true
        self.isBusy = false
    }
    
    func getTodoItems() -> Void {
        if (self.isBusy) { return }

        self.isBusy = true
        initialize { initError in
            if let initError = initError {
                self.setErrorCondition(initError)
            } else {
                table.read() { (result, readError) in
                    if let readError = readError {
                        self.setErrorCondition(readError)
                    } else if let serverItems = result?.items {
                        var fromServer = [TodoItem]()
                        for serverItem in serverItems {
                            fromServer.append(TodoItem(serverItem: serverItem))
                        }
                        self.items = fromServer
                        self.isBusy = false
                    } else {
                        self.errorMessage = "Result not returned"
                        self.hasError = true
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
                let clientItem = TodoItem(text: text)
                self.table.insert(clientItem.toDictionary()) { (result, insertError) in
                    if let insertError = insertError {
                        self.setErrorCondition(insertError)
                    } else if let serverItem = result {
                        self.items.append(TodoItem(serverItem: serverItem))
                    } else {
                        self.errorMessage = "Invalid response from server on insert"
                        self.hasError = true
                    }
                }
            }
        }
    }
    
    func toggleCompletion(_ item: TodoItem) {
        saveTodoItem(TodoItem(id: item.id, text: item.text, complete: !item.complete))
    }
    
    func saveTodoItem(_ item: TodoItem) {
        initialize { initError in
            if let initError = initError {
                self.setErrorCondition(initError)
            } else {
                self.table.update(item.toDictionary()) { (result, updateError) in
                    if let updateError = updateError {
                        self.setErrorCondition(updateError)
                    } else if let serverItem = result {
                        let clientItem = TodoItem(serverItem: serverItem)
                        if let index = self.items.firstIndex(where: { $0.id == clientItem.id }) {
                            self.items[index] = clientItem
                        } else {
                            self.errorMessage = "Item with id \(clientItem.id) does not exist"
                            self.hasError = true
                        }
                    } else {
                        self.errorMessage = "Invalid response from server on update"
                        self.hasError = true
                    }
                }
            }
        }
    }
}


