import Foundation

/**!
    The definition of a single TodoItem.  This must match the TodoItem definition on the server.
 */
struct TodoItem: Codable, Identifiable {
    /**!
        The unique ID of this item.  This is generally a UUID, but can be anything as long as it is
        guaranteed to be globally unique across all clients.
     */
    let id: String
    
    /**!
        The text for the item.
     */
    var text: String
    
    /**!
        Whether the item is completed or not.
     */
    var complete: Bool
    
    /**!
        Helper method to create a new TodoItem with some text.
     */
    static func newTodoItem(_ text: String, complete: Bool = false) -> TodoItem {
        return TodoItem(id: UUID().uuidString, text: text, complete: complete)
    }
}
