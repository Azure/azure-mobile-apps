import Foundation
import SwiftUI

func ColorFromRGB(_ rgb: UInt) -> Color {
    return Color(
        red:   Double((rgb & 0xFF0000) >> 16) / 255.0,
        green: Double((rgb & 0x00FF00) >>  8) / 255.0,
        blue:  Double(rgb & 0x0000FF) / 255.0
    )
}

struct Palette {
    /* Put all your colors here */
    static let border = ColorFromRGB(0x3867D5)
    
    /* And your gradients */
    static let headerGradient = Gradient(colors: [
        ColorFromRGB(0x3867D5),
        ColorFromRGB(0x81C7F5)
    ])
}
