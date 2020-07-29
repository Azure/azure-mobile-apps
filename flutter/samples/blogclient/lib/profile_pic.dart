import 'package:flutter/widgets.dart';

class ProfilePic extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return ClipRRect(
        borderRadius: BorderRadius.circular(10.0),
        child: Container(
            width: 50,
            height: 50,
            child: Image.network('https://www.fillmurray.com/100/101')));
  }
}
