//
//  TodoListView.swift
//  ZumoQuickstart
//
//  Created by Adrian Hall on 1/23/21.
//

import SwiftUI

struct TodoListView: View {
    var body: some View {
        VStack {
            // Header
            Text("Header")
            
            // Scrollable List Items
            Text("List Items")
            Spacer()
            
            // Entry Area
            Text("Entry Area")
        }
    }
}

struct TodoListView_Previews: PreviewProvider {
    static var previews: some View {
        TodoListView()
    }
}
