import Foundation

/**
 The definition of a single TodoItem.  This must match the TodoItem definition on the server.
 */
struct TodoItem: Codable, Identifiable {
    let id: String
    var text: String
    var complete: Bool
    
    /**
     Initialize a TodoItem without an ID.  This will create an ID for us.
     */
    init(text: String, complete: Bool = false) {
        self.id = UUID().uuidString
        self.text = text
        self.complete = complete
    }
    
    /**
     Initialize a TodoItem with an ID.
     */
    init(id: String, text: String, complete: Bool = false) {
        self.id = id
        self.text = text
        self.complete = complete
    }
    
    /**
     Initialize a TodoItem with data from the server
     */
    init(serverItem: [AnyHashable: Any]) {
        self.id = serverItem["id"] as! String
        self.text = serverItem["Text"] as! String
        self.complete = serverItem["Complete"] as! Bool
    }
    
    /**
     Convert a TodoItem to the AnyHashable dictionary form required by the server.
     */
    func toDictionary() -> [String: Any] {
        var serverItem = [String: Any]()
        serverItem["id"] = self.id
        serverItem["Text"] = self.text
        serverItem["Complete"] = self.complete
        return serverItem
    }
}
