import Foundation
import MicrosoftAzureMobile

class TodoService : ObservableObject {
    /**
    Singleton service for the TodoService.  A service should almost always be a singleton so that caching, authentication, and such like will always work.
     */
    static let shared = TodoService()
    
    /**
     Azure Mobile Apps - the client for the backend
     */
    private var client: MSClient
    
    /**
     Azure Mobile Apps - the table reference
     */
    private var table: MSTable
    
    /**
     Initialize the client.
     */
    private init() {
        self.client = MSClient(applicationURLString: BackendUrl)
        self.table = self.client.table(withName: "TodoItem")
    }
    
    func getAllItems() {
        
    }
    
    func deleteItem(_ id: String) {
        
    }
    
    func addItem(_ item: TodoItem) {
        
    }
    
    func addItem(_ text: String) {
        
    }
    
    func updateItem(_ item: TodoItem) {
        
    }
}
