import Foundation
import UIKit

protocol ToDoItemDelegate {
    func didSaveItem(_ text : String)
}

class ToDoItemViewController: UIViewController,  UIBarPositioningDelegate, UITextFieldDelegate {
    @IBOutlet weak var text: UITextField!
    
    var delegate : ToDoItemDelegate?
    
    override func viewDidLoad()
    {
        super.viewDidLoad()
        
        self.text.delegate = self
        self.text.becomeFirstResponder()
    }
    
    @IBAction func cancelPressed(_ sender : UIBarButtonItem) {
        self.text.resignFirstResponder()
    }
    
    @IBAction func savePressed(_ sender : UIBarButtonItem) {
        saveItem()
        self.text.resignFirstResponder()
    }
    
    // Textfield
    
    func textFieldDidEndEditing(_ textField: UITextField)
    {
        _ = self.navigationController?.popViewController(animated: true)
    }
    
    func textFieldShouldEndEditing(_ textField: UITextField) -> Bool
    {
        return true
    }
    
    func textFieldShouldReturn(_ textField: UITextField) -> Bool
    {
        saveItem()
        
        textField.resignFirstResponder()
        return true
    }
    
    // Delegate
    
    func saveItem()
    {
        if let text = self.text.text {
            self.delegate?.didSaveItem(text)
        }
    }
}