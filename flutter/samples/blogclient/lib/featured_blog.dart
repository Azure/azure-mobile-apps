import 'package:flutter/material.dart';
import 'profile_pic.dart';
import 'package:timeago/timeago.dart';

class FeaturedBlog extends StatelessWidget {
  final String title;
  final String author;
  final String profilePic;
  final String backgroundPic;
  final DateTime createdDate;

  const FeaturedBlog({
    Key key,
    this.title,
    this.author,
    this.createdDate,
    this.profilePic,
    this.backgroundPic,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 332,
      height: 332,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(20.0),
        child: Stack(
          children: <Widget>[
            Image.network(
              backgroundPic,
              fit: BoxFit.fill,
            ),
            Container(
              color: Color(0x88000000),
            ),
            Padding(
              padding: const EdgeInsets.all(20.0),
              child: Align(
                child: Icon(
                  Icons.bookmark_border,
                  color: Colors.white,
                  size: 30,
                ),
                alignment: Alignment.topRight,
              ),
            ),
            Padding(
              padding: EdgeInsets.all(10),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.end,
                crossAxisAlignment: CrossAxisAlignment.start,
                children: <Widget>[
                  Text(
                    title,
                    style: TextStyle(color: Colors.white, fontSize: 30),
                  ),
                  Row(
                    children: <Widget>[
                      ProfilePic(),
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: <Widget>[
                          Text(
                            author,
                            style: TextStyle(color: Colors.white, fontSize: 20),
                          ),
                          Row(
                            children: <Widget>[
                              Icon(
                                Icons.timer,
                                color: Colors.white,
                              ),
                              Text(
                                createdDate.toString(),
                                style: TextStyle(
                                    color: Colors.white, fontSize: 12),
                              ),
                            ],
                          ),
                        ],
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
