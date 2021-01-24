//
//  CheckItem.swift
//  ZumoQuickstart
//
//  Created by Adrian Hall on 1/24/21.
//

import SwiftUI

struct CheckItem: View {
    private let icon: String
    private let color: Color
    private let onChange: (() -> Void)?
    
    init(_ isChecked: Bool, onChange: (() -> Void)? = nil) {
        self.icon = isChecked ? "checkmark.circle.fill" : "circle"
        self.color = isChecked ? Color.green : Color.orange
        self.onChange = onChange
    }
    
    func triggerOnChange() {
        if let trigger = self.onChange {
            trigger()
        }
    }
    
    var body: some View {
        Image(systemName: icon)
            .resizable()
            .frame(width: 20, height: 20)
            .foregroundColor(self.color)
            .onTapGesture { triggerOnChange() }
    }
}

#if DEBUG
struct CheckItem_Previews: PreviewProvider {
    static var previews: some View {
        VStack {
            CheckItem(false)
            CheckItem(true)
        }
    }
}
#endif
