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
