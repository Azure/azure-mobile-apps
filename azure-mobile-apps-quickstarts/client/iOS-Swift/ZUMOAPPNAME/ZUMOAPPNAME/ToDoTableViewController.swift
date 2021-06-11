// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
import Foundation
import UIKit
import CoreData

class ToDoTableViewController: UITableViewController, NSFetchedResultsControllerDelegate, ToDoItemDelegate {
    
    var table : MSSyncTable?
    var store : MSCoreDataStore?
    
    lazy var fetchedResultController: NSFetchedResultsController<NSFetchRequestResult> = {
        let fetchRequest:NSFetchRequest<NSFetchRequestResult> = NSFetchRequest(entityName: "TodoItem")
        let managedObjectContext = (UIApplication.shared.delegate as! AppDelegate).managedObjectContext!
        
        // show only non-completed items
        fetchRequest.predicate = NSPredicate(format: "complete != true")
        
        // sort by item text
        fetchRequest.sortDescriptors = [NSSortDescriptor(key: "createdAt", ascending: true)]
        
        // Note: if storing a lot of data, you should specify a cache for the last parameter
        // for more information, see Apple's documentation: http://go.microsoft.com/fwlink/?LinkId=524591&clcid=0x409
        let resultsController = NSFetchedResultsController(fetchRequest: fetchRequest, managedObjectContext: managedObjectContext, sectionNameKeyPath: nil, cacheName: nil)
        
        resultsController.delegate = self;
        
        return resultsController
    }()
    
    override func viewDidLoad() {
        super.viewDidLoad()
        // Do any additional setup after loading the view, typically from a nib.
        
        let client = MSClient(applicationURLString: "ZUMOAPPURL")
        let managedObjectContext = (UIApplication.shared.delegate as! AppDelegate).managedObjectContext!
        self.store = MSCoreDataStore(managedObjectContext: managedObjectContext)
        client.syncContext = MSSyncContext(delegate: nil, dataSource: self.store, callback: nil)
        self.table = client.syncTable(withName: "TodoItem")
        self.refreshControl?.addTarget(self, action: #selector(ToDoTableViewController.onRefresh(_:)), for: UIControlEvents.valueChanged)
        
        var error : NSError? = nil
        do {
            try self.fetchedResultController.performFetch()
        } catch let error1 as NSError {
            error = error1
            print("Unresolved error \(error), \(error?.userInfo)")
            abort()
        }

        // Refresh data on load
        self.refreshControl?.beginRefreshing()
        self.onRefresh(self.refreshControl)
    }
    
    func onRefresh(_ sender: UIRefreshControl!) {
        UIApplication.shared.isNetworkActivityIndicatorVisible = true
        
        self.table!.pull(with: self.table?.query(), queryId: "AllRecords") {
            (error) -> Void in
            
            UIApplication.shared.isNetworkActivityIndicatorVisible = false
            
            if error != nil {
                // A real application would handle various errors like network conditions,
                // server conflicts, etc via the MSSyncContextDelegate
                print("Error: \((error! as NSError).description)")
                
                // We will just discard our changes and keep the servers copy for simplicity                
                if let opErrors = (error! as NSError).userInfo[MSErrorPushResultKey] as? Array<MSTableOperationError> {
                    for opError in opErrors {
                        print("Attempted operation to item \(opError.itemId)")
                        if (opError.operation == MSTableOperationTypes() || opError.operation == .delete) {
                            print("Insert/Delete, failed discarding changes")
                            opError.cancelOperationAndDiscardItem(completion: nil)
                        } else {
                            print("Update failed, reverting to server's copy")
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
    
    override func tableView(_ tableView: UITableView, canEditRowAt indexPath: IndexPath) -> Bool
    {
        return true
    }
    
    override func tableView(_ tableView: UITableView, editingStyleForRowAt indexPath: IndexPath) -> UITableViewCellEditingStyle
    {
        return UITableViewCellEditingStyle.delete
    }
    
    override func tableView(_ tableView: UITableView, titleForDeleteConfirmationButtonForRowAt indexPath: IndexPath) -> String?
    {
        return "Complete"
    }
    
    override func tableView(_ tableView: UITableView, commit editingStyle: UITableViewCellEditingStyle, forRowAt indexPath: IndexPath)
    {
        let record = self.fetchedResultController.object(at: indexPath) as! NSManagedObject
        var item = self.store!.tableItem(from: record)
        item["complete"] = true
        
        UIApplication.shared.isNetworkActivityIndicatorVisible = true
        
        self.table!.update(item) { (error) -> Void in
            UIApplication.shared.isNetworkActivityIndicatorVisible = false
            if error != nil {
                print("Error: \((error! as NSError).description)")
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
    
    
    // MARK: - ToDoItemDelegate
    
    
    func didSaveItem(_ text: String)
    {
        if text.isEmpty {
            return
        }
        
        // We set created at to now, so it will sort as we expect it to post the push/pull
        let itemToInsert = ["text": text, "complete": false, "__createdAt": Date()] as [String : Any]
        
        UIApplication.shared.isNetworkActivityIndicatorVisible = true
        self.table!.insert(itemToInsert) {
            (item, error) in
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
                // note: Apple samples show a call to configureCell here; this is incorrect--it can result in retrieving the
                // wrong index when rows are reordered. For more information, see:
                // http://go.microsoft.com/fwlink/?LinkID=524590&clcid=0x409
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
