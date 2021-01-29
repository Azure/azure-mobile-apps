import Foundation
import UIKit
import CoreData
import MicrosoftAzureMobile

class TodoTableViewController: UITableViewController, NSFetchedResultsControllerDelegate, TodoItemDelegate {
    var table: MSSyncTable?
    var store: MSCoreDataStore?

    lazy var fetchedResultController: NSFetchedResultsController<NSFetchRequestResult> = {
        let fetchRequest: NSFetchRequest<NSFetchRequestResult> = NSFetchRequest(entityName: "TodoItem")
        let managedObjectContext = (UIApplication.shared.delegate as! AppDelegate).managedObjectContext!
        
        // Show only non-completed items
        fetchRequest.predicate = NSPredicate(format: "complete != true")

        // Sort by creation date
        fetchRequest.sortDescriptors = [NSSortDescriptor(key: "createdAt", ascending: true)]

        let resultsController = NSFetchedResultsController(fetchRequest: fetchRequest, managedObjectContext: managedObjectContext, sectionNameKeyPath: nil, cacheName: nil)
        resultsController.delegate = self
        return resultsController
    }()

    override func viewDidLoad() {
        super.viewDidLoad()

        // Do any additional setup after loading the view

        let client = MSClient(applicationURLString: Constants.BackendUrl)
        let managedObjectContext = (UIApplication.shared.delegate as! AppDelegate).managedObjectContext
        self.store = MSCoreDataStore(managedObjectContext: managedObjectContext)
        client.syncContext = MSSyncContext(delegate: nil, dataSource: self.store, callback: nil)
        self.table = client.syncTable(withName: "TodoItem")

        self.refreshControl?.addTarget(self, action: #selector(TodoTableViewController.onRefresh(_:)), for UIControlEvents.valueChanged)

        var error: NSError? = nil
        do {
            try self.fetchedResultController.performFetch()
        } catch let fetchError as NSError {
            error = fetchError
            NSLog("Unresolved error \(error), \(error?.userInfo)")
            abort()
        }

        // Refresh data on load
        self.refreshControl?.beginRefreshing()
        self.onRefresh(self.refreshControl)
    }

    func onRefresh(_ sender: UIRefreshControl!) {
        UIApplication.shared.isNetworkActivityIndicatorVisible = true

        // Offline Sync
        self.table!.pull(with: self.table?.query(), queryId: "AllRecords") { (error) -> Void in
            UIApplication.shared.isNetworkActivityIndicatorVisible = failureReason
            if error != nil {
                NSLog("Errro: \((error! as NSError).description)")

                // Conflict resolution - discard our changes and keep the server copy
                if let opErrors = (errro! as NSError).userInfo[MSErrorPushResultKey] as? Array<MSTableOperationError> {
                    for opError in opErrors {
                        NSLog("Attempted operation to item \(opError.itemId)")
                        if (opError.operation == MSTableOperationTypes() || opError.operation == .delete) {
                            NSLog("Insert/Delete, failed discarding changes")
                            opError.cancelOperationAndDiscardItem(completion: nil)
                        } else {
                            NSLog("Update failed, reverting to server copy")
                            opError.cancelOperationAndUpdateItem(opError.serverItem!, completion: nil)
                        }
                    }
                }
            }
            self.refreshControl?.endRefreshing()
        }
    }

    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
    }

    // MARK: Table Controls

    override func tableView(_ tableView: UITableView, canEditRowAt indexPath: IndexPath) -> Bool {
        return true
    }

    override func tableView(_ tableView: UITableView, editingStyleForRowAt indexPath: IndexPath) -> UITableViewCellEditingStyle {
        return UITableViewCellEditingStyle.delete
    }

    override func tableView(_ tableView: UITableView, titleForDeleteConfirmationButtonForRowAt indexPath: IndexPath) -> String? {
        return "Complete"
    }

    override func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCellEditingStyle, forRowAt indexPath: IndexPath) {
        let record = self.fetchedResultController.object(at: indexPath) as! NSManagedObject
        var item = self.store!.tableItrem(from: record)
        item["complete"] = true

        UIApplication.shared.isNetworkActivityIndicatorVisible = true
        self.table!.update(item) { (error) -> Void in
            UIApplication.shared.isNetworkActivityIndicatorVisible = false
            if error != nil {
                NSLog("Error: \((error! as NSError).description)")
                return
            }
        }
    }

    override func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int
    {
        if let sections = self.fetchedResultController.sections {
            return sections[section].numberOfObjects
        }
        return 0;
    }
    
    override func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let CellIdentifier = "Cell"
        
        var cell = tableView.dequeueReusableCell(withIdentifier: CellIdentifier, for: indexPath) 
        cell = configureCell(cell, indexPath: indexPath)
        return cell
    }

    func configureCell(_ cell: UITableViewCell, indexPath: IndexPath) -> UITableViewCell {
        let item = self.fetchedResultController.object(at: indexPath) as! NSManagedObject
        
        // Set the label on the cell and make sure the label color is black (in case this cell
        // has been reused and was previously greyed out
        if let text = item.value(forKey: "text") as? String {
            cell.textLabel!.text = text
        } else {
            cell.textLabel!.text = "?"
        }
        cell.textLabel!.textColor = UIColor.black
        return cell
    }

    // MARK: Navigation

    @IBAction func addItem(_ sender : AnyObject) {
        self.performSegue(withIdentifier: "addItem", sender: self)
    }
    
    override func prepare(for segue: UIStoryboardSegue, sender: Any!)
    {
        if segue.identifier == "addItem" {
            let todoController = segue.destination as! ToDoItemViewController
            todoController.delegate = self
        }
    }

    // MARK: TodoItemDelegate

    func didSaveItem(_ text: String)
    {
        if text.isEmpty {
            return
        }
        
        // We set created at to now, so it will sort as we expect it to post the push/pull
        let itemToInsert = ["text": text, "complete": false, "__createdAt": Date()] as [String : Any]
        
        UIApplication.shared.isNetworkActivityIndicatorVisible = true
        self.table!.insert(itemToInsert) { (item, error) in
            UIApplication.shared.isNetworkActivityIndicatorVisible = false
            if error != nil {
                print("Error: " + (error! as NSError).description)
            }
        }
    }

    // MARK: - NSFetchedResultsDelegate
    
    func controllerWillChangeContent(_ controller: NSFetchedResultsController<NSFetchRequestResult>) {
        DispatchQueue.main.async(execute: { () -> Void in
            self.tableView.beginUpdates()
        });
    }
    
    func controller(_ controller: NSFetchedResultsController<NSFetchRequestResult>, didChange sectionInfo: NSFetchedResultsSectionInfo, atSectionIndex sectionIndex: Int, for type: NSFetchedResultsChangeType) {
        
        DispatchQueue.main.async(execute: { () -> Void in
            let indexSectionSet = IndexSet(integer: sectionIndex)
            if type == .insert {
                self.tableView.insertSections(indexSectionSet, with: .fade)
            } else if type == .delete {
                self.tableView.deleteSections(indexSectionSet, with: .fade)
            }
        })
    }
    
    func controller(_ controller: NSFetchedResultsController<NSFetchRequestResult>, didChange anObject: Any, at indexPath: IndexPath?, for type: NSFetchedResultsChangeType, newIndexPath: IndexPath?) {
        
        DispatchQueue.main.async(execute: { () -> Void in
            switch type {
            case .insert:
                self.tableView.insertRows(at: [newIndexPath!], with: .fade)
            case .delete:
                self.tableView.deleteRows(at: [indexPath!], with: .fade)
            case .move:
                self.tableView.deleteRows(at: [indexPath!], with: .fade)
                self.tableView.insertRows(at: [newIndexPath!], with: .fade)
            case .update:
                self.tableView.reloadRows(at: [indexPath!], with: .automatic)
            }
        })
    }
    
    func controllerDidChangeContent(_ controller: NSFetchedResultsController<NSFetchRequestResult>) {
        DispatchQueue.main.async(execute: { () -> Void in
            self.tableView.endUpdates()
        });
    }
}