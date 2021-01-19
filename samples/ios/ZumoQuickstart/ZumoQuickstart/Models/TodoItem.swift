//
//  TodoItem.swift
//  ZumoQuickstart
//
//  Created by Adrian Hall on 1/19/21.
//

import Foundation

struct TodoItem : Codable, Identifiable {
    var id: String
    var text: String
    var complete: Bool
}
