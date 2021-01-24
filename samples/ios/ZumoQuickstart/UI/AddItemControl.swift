//
//  AddItemControl.swift
//  ZumoQuickstart
//
//  Created by Adrian Hall on 1/23/21.
//

import SwiftUI

struct AddItemControl: View {
    private let onCommit: ((String) -> Void)?
    
    @State var text = ""
    
    init(onCommit: ((String) -> Void)? = nil) {
        self.onCommit = onCommit
    }
    
    func triggerOnCommit() {
        if let trigger = self.onCommit {
            trigger(self.text)
        }
        self.text = ""
    }
    
    var body: some View {
        HStack {
            Image(systemName: "plus.circle.fill")
                .resizable()
                .frame(width: 20, height: 20)
                .foregroundColor(Palette.border)
            TextField("Enter some text",
                      text: $text,
                      onCommit: { triggerOnCommit() })
        }
        .padding(.all, 4)
        .overlay(RoundedRectangle(cornerRadius: 16.0).stroke(Palette.border, lineWidth: 1))
        .padding(.horizontal, 4)
    }
}

#if DEBUG
struct AddItemControl_Previews: PreviewProvider {
    static var previews: some View {
        VStack {
            Header()
            Spacer()
            AddItemControl()
        }
    }
}
#endif
