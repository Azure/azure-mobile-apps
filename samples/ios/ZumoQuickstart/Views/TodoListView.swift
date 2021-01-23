//
//  TodoListView.swift
//  ZumoQuickstart
//
//  Created by Adrian Hall on 1/23/21.
//

import SwiftUI

struct TodoListView: View {
    func refreshData() -> Void {
        // Refresh data
    }
    
    var body: some View {
        VStack {
            Header()
            TodoItemList()
            Spacer()
            Text("Entry Area")
        }
    }
}

struct TodoListView_Previews: PreviewProvider {
    static var previews: some View {
        TodoListView()
    }
}
