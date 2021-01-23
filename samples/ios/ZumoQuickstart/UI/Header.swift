import SwiftUI

struct HeaderBackground: View {
    let background = LinearGradient(
        gradient: Palette.headerGradient,
        startPoint: .topLeading,
        endPoint: .bottomTrailing
    )
    
    var body: some View {
        Group {
            Rectangle()
                .fill(background)
                .edgesIgnoringSafeArea(.top)
            Circle()
                .frame(width: 180, height: 180)
                .foregroundColor(.white)
                .opacity(0.17)
                .position(x: 0, y: 0)
            GeometryReader { g in
                Circle()
                    .frame(width: 92, height: 92)
                    .foregroundColor(.white)
                    .opacity(0.12)
                    .position(x: g.size.width - 18, y: 20)
            }
        }
    }
}

struct Header: View {
    var body: some View {
        ZStack {
            HeaderBackground()
            HStack {
                Text("Azure Mobile Apps")
                    .font(.title)
                    .foregroundColor(.white)
                    .padding(8)
                Spacer()
            }
        }.frame(height: 50)
    }
}

#if DEBUG
struct HeaderBackground_Previews: PreviewProvider {
    static var previews: some View {
        VStack {
            Header()
            Spacer()
        }
    }
}
#endif
